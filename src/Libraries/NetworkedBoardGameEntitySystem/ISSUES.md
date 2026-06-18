# NetworkedBoardGameEntitySystem — Audit Findings

A deep audit of the `NetworkedBoardGameEntitySystem` library. Issues are ordered by
severity. Line references are to the state of the files at audit time.

---

## High

### H2. `Server.ReceiveJsonPacket` does not compile in non-DEBUG builds
**File:** `Server.cs:26-50`

```csharp
protected override NetworkResponse? ReceiveJsonPacket(JsonPacket jsonPacket)
{
    switch (jsonPacket.Type)
    {
        case nameof(EditorPacket): ... return ...;   // both try/catch return
            break;                                    // unreachable
#if DEBUG
        default: throw new NotImplementedException(...);
#else
        default: break;                               // falls out of switch
#endif
    }
    // no return statement here
}
```

In a `#if DEBUG` build every path returns or throws, so it compiles. In a release build the
`default: break;` falls out of the switch to the end of a non-`void` method with no return —
a "not all code paths return a value" compile error. The unreachable `break;` after the
`case` returns is also a warning. The method needs a trailing `return null;`.

### RESERVED FOR FIXING LATER, IGNORE FOR NOW - H3. No information filtering — all hidden state is broadcast to every client
**Files:** `Server.cs:58-79` (`PushUpdate`, `OnPeerConnected`), `EntityGameData.cs:44-56`

`CommitState`/`ForceSetValue` push each update to **all** connected peers via
`Server.PushUpdate` (which loops over every `NetPeer`), and `OnPeerConnected` replays the
**entire** history of **every** field of **every** entity to a joiner. There is no
per-client visibility filtering anywhere.

`CLAUDE.md` states the core design premise is "authoritative server, per-client state…
because of hidden information, a client's view may legitimately differ from the server's."
As written, that premise is violated: a client receives the full serialized value and
complete history of every entity, so any hidden information (face-down cards, fog-of-war,
opponents' resources) is fully visible to anyone inspecting network traffic or client state.
This is both a correctness and a cheating/security concern and likely needs a per-field
visibility model before the game logic is built on top.

### FIXED - H5. Response to a fire-and-forget `SendRequest` dereferences a null `TaskCompletionSource`
**Files:** `NetworkPeer.cs:128-133`, `SentRequest.cs:10,16`

`OnNetworkReceive` completes the awaiting task for an incoming response with:

```csharp
originalRequest.TCS.SetResult(receivedResponse);
```

But `TCS` is only populated when a request is sent via `SendRequestAwaitCallback`. The
fire-and-forget `SendRequest` (used by the editor's `DeleteEntity` and `EditValue` —
`TTRenderer_Editor.cs:65,78`) builds its `SentRequest` with the default `tcs = null`. When
the server's response to one of those arrives, `originalRequest.TCS` is `null` and
`SetResult` throws a `NullReferenceException`. That throw is **not** inside the surrounding
try/catch (which only guards the request-handling `else` branch), so it propagates out of
`OnNetworkReceive` → `PollEvents` → `NetworkController._Process` unhandled, and also skips
the trailing `reader.Recycle()`. Guard the completion and prefer `TrySetResult` so a
duplicate response can't throw either: `originalRequest.TCS?.TrySetResult(receivedResponse);`.

---

## Medium

### FIXED - M3. Editor entity-deletion leaves a dangling reference in `EntitiesByType`
**Files:** `ServerGameState.cs:85-92`, `GameEntity.cs:48,51`

Per the audit note, `EditorPacket`s are an out-of-band scenario-editing path, so the
never-destroy invariant and the lack of client propagation are accepted here. The remaining
"other breaking behavior" is: `HandleEditorPacket` deletes via
`EntitiesById.Remove(editorPacket.EntityId)` **only**, but `GameEntity.Setup` registers
every entity in *both* `EntitiesById` and `EntitiesByType` (`Add_CertainOfKey`). The
deletion never removes it from `EntitiesByType`, so afterwards:
- any query by type still returns the deleted entity (a dangling reference), and
- the type set can accumulate dead instances across repeated create/delete edits.

The deletion path should also remove the entity from `EntitiesByType` (and any other
indices) so the state stays consistent after an editor delete.

### FIXED - M4. Editor packets can still force a future id collision
**Files:** `ServerGameState.cs:58-68`, `49-56`

`InstantiateGameEntity(Type, id)` now rejects an id that is negative or already present
(`EntitiesById.ContainsKey(id)` → throw), closing the *immediate* collision. But the deeper
problem M4 described is unfixed: a caller-supplied id (e.g. from an `EditorPacket`) does
**not** advance `_idTicker`. Create an entity at id 50 while the ticker is at 3, and the
ticker keeps handing out 3, 4, … until it reaches 50 — at which point the generic
`InstantiateGameEntity<T>()` (which does *no* collision check at all, line 52) calls
`Setup`, whose `EntitiesById.Add(50, …)` throws `ArgumentException`. Reserve/advance the
ticker past any externally-supplied id (`_idTicker = Math.Max(_idTicker, id + 1)`).

---

## Low

### L1. `SendRequestAwaitCallback`'s awaiting task still hangs forever on timeout
**Files:** `NetworkPeer.cs:23-48,75-90`, `NetworkController.cs:43`

`MonitorTimeouts()` (pumped every frame from `NetworkController._Process`) now prunes stale
`SentRequests` after `TimeoutSeconds` and disconnects the peer, so the dictionary no longer
leaks. However it never completes the request's `TaskCompletionSource`: for a
`SendRequestAwaitCallback` caller (e.g. `TTRenderer_Editor.cs:30`) the
`await sentRequest.TCS.Task` never returns, so the awaiting task hangs forever even after
the entry is reaped. On timeout the TCS should be faulted/cancelled
(`TrySetException`/`TrySetCanceled`).

Secondary bug: `MonitorTimeouts` handles only one timed-out request per call. The in-loop
`if (_deadRequests.Any()) _deadRequests.Clear();` discards everything collected so far each
time another expired request is found, so when several time out in the same frame only the
last is removed/disconnected and the rest wait for subsequent frames.

### L2. `Client.OnPeerConnected` discards the existing game state with no renderer refresh
**File:** `Client.cs:83-86`

Each (re)connect assigns a brand-new `ClientGameState`, dropping any prior entities. The
renderer is never told to `FullRefresh()`, and the new state starts at `GameStepID == 0`
(compounding C1). On reconnect, stale renderer objects linger and `EntitiesChanged` is not
reset. Consider reusing/clearing state and invoking `FullRefresh()`.

### FIXED - L5. `RefreshState` never notifies when scrubbing to before an entity existed
**File:** `EntityGameData.cs:158-165`

```csharp
DeserializeDataAndSetStateToIt(null);
_currentSerializedState = null;
if (_currentSerializedState != null) NotifyEntityStateChanged();  // always false
_currentSerializedState = null;
```

`_currentSerializedState` is set to `null` *before* the change check, so the condition is
always false and `NotifyEntityStateChanged()` never fires in this branch. When an entity is
viewed at a step before its first history frame (e.g. time-scrubbing via `SetStateToStep`),
the data resets to default but the renderer is never told, so it keeps showing the old
value. Capture the previous `_currentSerializedState` and notify if it was non-null before
reassigning. (The analogous M6 bug on the main branches — assigning `_historyFrames[0].Data`
instead of the shown frame — has been fixed; this branch was missed.)

### L6. `MonitorTimeouts` NREs on a request sent while disconnected (`Destination == null`)
**Files:** `NetworkPeer.cs:42,64-90`, `SentRequest.cs:10,12`, `Client.cs:16`, `TTRenderer_Editor.cs:38-110,219`

`SentRequest.Destination` is set once from the `destination` argument passed to
`SendRequest` / `SendRequestAwaitCallback`. Every editor caller passes
`GameState.Client.ConnectedServer`, which is `_netManager.FirstPeer` (`Client.cs:16`) —
i.e. `(NetPeer)_headPeer`, which is **`null` whenever the client has no peer** (before
connecting, mid-handshake, or after a disconnect/timeout). So triggering any editor action
(create/edit/delete/import) while not connected enqueues a `SentRequest` with a `null`
`Destination`. Thirty seconds later `MonitorTimeouts` reaps it and dereferences the null:

```csharp
if (originalRequest.Destination.ConnectionState == ConnectionState.Connected)  // NRE
    originalRequest.Destination.Disconnect();
```

This is distinct from a *stale* peer: a peer that disconnects after a request is sent leaves
`Destination` non-null (it still holds the original reference), so reading `ConnectionState`
is safe. The null specifically comes from a request created while `FirstPeer` was null.

Fix in two layers: (1) reject a `null` destination at the source in `SendRequest` /
`SendRequestAwaitCallback` (drop or return an error `NetworkResponse` so an awaiting caller
doesn't hang — ties into L1); and (2) make the reap null-safe regardless, e.g.
`if (originalRequest.Destination is { ConnectionState: ConnectionState.Connected })`. While
here, also fix the L1 secondary bug on the line above (the in-loop
`if (_deadRequests.Any()) _deadRequests.Clear();` discards all but the last dead request each
frame, and the reaped request's `TCS` is never faulted).
