# 6. Combat Phase (§12.0)

The second phase of a **Player Turn**, immediately after Movement. Battles occur
wherever Enemy units occupy the same area. Battles in Land Areas are **Land
Battles**; those in Sea Areas are **Sea Battles**.

## Sequence

1. **Designate Active Battles** (§12.1)
2. **Resolve Battles** one at a time, in the Active Player's chosen order
   (§12.2), each in **Combat Rounds** (§12.21)
3. Apply **Combat Actions** (Fire or Retreat) by unit type (§12.3)
4. **End / resolve** Battles and return units appropriately

---

## 12.1 Active Battles

At the start of the Combat Phase the Active Player **designates** which Battles
to resolve, turning all units there face-up (these are the **Active Battles**).

- The Active Player **may** resolve any or all current Battles, but **must**
  resolve Battles where they have Aggressed (§11.54) this Player Turn.
- The Active Player is the **Attacker** in all Active Battles; the opponent is
  the **Defender**. (Attacker/Defender are *not* the same as Aggressor/Owner.)

## 12.2 Resolving Battles

Resolve Active Battles one at a time in an order chosen by the Active Player.

- **Land Battles** are resolved only **one Combat Round per turn** — they are not
  necessarily settled in one Player Turn and may continue across turns.
- **Sea Battles** are resolved to a **conclusion** — Combat Rounds repeat until
  the Sea Battle ends.

### 12.21 Combat Rounds
A Combat Round is one cycle in which each unit, in order by unit type, takes one
**Combat Action**.

## 12.3 Combat Actions

In a Combat Round, each unit takes a Combat Action in order **from the top down**
of the Unit Table (Fortresses first, then AFs, … Infantry last; Convoys get no
Combat Action — §12.72).

- Within a unit type, **Defenders go before Attackers** — *unless* Attacking
  units have **FirstFire** capability (§9.0) and the Defenders do not.
- An individual unit's Combat Action is either to **Fire** (§12.4) or **Retreat**
  (§12.5).
- A Land Battle's Combat Round ends when all units have acted (concluding that
  Battle's resolution for the turn). A Sea Battle continues Combat Rounds until
  one side Retreats, Escapes, or is eliminated.

## 12.4 Unit Fire

A Firing unit rolls Combat dice attempting to damage Enemy units.

- **12.41 Targeting:** before firing, declare a **Target Class** — **Air (A)**,
  **Naval (N)**, **Ground (G)**, or **Submarine (S)**.
- **12.42 Firepower:** a unit's Firepower vs. a Target Class is the number after
  that class letter (e.g. "G3" = scores a Hit on a die roll of 1–3 vs. Ground).
- **12.43 Firing:** roll a number of dice equal to the unit's **CV**; each die
  ≤ Firepower is a **Hit**.
- **12.44 Applying Hits:** each Hit reduces the **strongest (largest CV)** Enemy
  unit of the Targeted Class by **1 CV** (Carriers/Convoys lose **2 CV** per
  Hit). Units reduced to **0 CV** are eliminated (may be rebuilt as Cadres).
  Excess Hits beyond the Target Class are lost.

## 12.5 Retreats

Instead of firing, a unit may **Retreat** into an adjacent **Friendly** area
(including Open Seas), returning upright. Retreats obey Disengaging rules
(§11.51) including Border Limits. Units **cannot** Retreat into Enemy, Disputed,
Neutral, or areas where the Enemy Engaged them.

- **12.51 Retreat by ReBasing:** ANS units may Retreat by **ReBasing** (§13.0) to
  an undisputed Friendly Land Area in movement range. **Air units *must* Retreat
  by ReBasing** and are eliminated if unable.
- **12.52 ANS Forced Retreats:** ANS units sometimes **must** Retreat — e.g. from
  Enemy Territory without Ground Support, and (at end of a Sea Combat Round) all
  participating Air units of **both** sides must ReBase.

---

## 12.6 Special Land Combat
- **12.61 Invasion Battles:** Sea-Invaded Ground units placed face-down have no
  Combat Action that turn; they can still **take Hits** (revealed, reduced, then
  returned upright).
- **12.62 Strategic Bombing:** Air Forces with **Precision Bombsight** Tech can
  attack Enemy **IND** in (over) the Enemy MainCapital — Firepower I1, each Hit
  *permanently* reduces Enemy IND by 1.
- **12.63 Raid Battles:** ANS Aggression with no Ground Support — like normal
  Battles but they don't block Enemy Retreats; Raiders must Retreat at the end of
  the Combat Round.

## 12.7 Special Sea Combat
- **12.71 BattleGroups:** only one BattleGroup per side joins a Sea Battle per
  Combat Round; a newly-engaged BattleGroup must join at the beginning of a
  Round.
- **12.72 Convoys:** get no Combat Action; targeted as Naval ('N'), losing
  **2 CV per Hit**.
- **12.73 Submarine Escape:** at the end of any Sea Combat Round, Subs may
  **Escape** by Disengaging downwards (turn the Sub face-down in that Sea Area).
- **12.74 Carrier Strikes ("Shoot & Scoot"):** a Carrier may optionally Fire N1
  at Enemy Naval units **and immediately Retreat/ReBase** as one Combat Action.
- **12.75 Air Units:** at the end of a Sea Combat Round, all Air units that
  joined the Battle **must ReBase** (§13.2).

## 12.22–12.24 Ending Battles
- **Land (12.22):** at the end of a Combat Round, all ANS without Ground Support
  must Retreat; Owners' units return upright; Aggressor units stay face-up.
- **Sea (12.23):** all Air units must ReBase; Subs may Escape; unless ended,
  start another Combat Round.
- **Ending (12.24):** a Battle ends immediately when only one side's units remain
  (ignoring Escaped Subs). Surviving units return upright and forego untaken
  Combat Actions.

> **3-Way Battles:** when all three Factions have units in the same area, at the
> start of Battle Resolution each Faction must specify which Enemy Faction(s)
> will **not** be targeted (§12.44 sidebar).

---

## 13.0 ReBasing (closely tied to Combat)

ANS units **ReBase** instead of Retreating: a **free move** (no Command needed)
out of a Disputed area into an undisputed Friendly-controlled Land Area, using
normal movement rules. AF units **must** ReBase rather than Retreat (§12.51);
mandatory AF ReBasing happens at the end of every Sea Combat Round (§13.2).

---

After all Active Battles are resolved, the Combat Phase ends and the next Player
Turn (in Command Order) begins. After all Player Turns, the Season ends with the
**Supply Phase** (`07 - Supply Phase.md`) for factions at War.
