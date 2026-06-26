# Phase / Game Behavior Implementation Checklist

Tracks which phases of a Game-Year (and their supporting `GameBehavior`s and
`IPlayerAction`s) are implemented. Derived from the phase docs in this folder and
the T&T 2nd-Ed rules. Section numbers (§) reference the rulebook.

**Legend:** `[x]` done · `[ ]` not started · each phase notes the
behavior(s)/action(s) that still need building.

---

## Infrastructure (cross-phase)

- [x] **Phase/turn tracking** — `TTSyncronizationBehavior` (year, season, subphase, phasing faction)
- [x] **Faction setup** — `FactionsBehavior` (instantiates factions, assigns nations)
- [ ] **Turn Order vs. Command Order** sequencing helpers (§7.14 / §10.1) — currently only `PhasingFaction` exists; no ordering logic
- [ ] **Subphase advancement wiring** — `OnPhaseTickerAdvancing()` is empty in every behavior; nothing drives the game from one subphase to the next except the Setup→NewYear hop in `PlacementBehavior.OnFactionPlaced()`

---

## Pre-Game

### [x] Setup (§5.0)
- [x] `PlacementBehavior` — seeds `UnitPlacement` entities, exposes `InitialPlacementAction`
- [x] `InitialPlacementAction` (`IPlacementAction`) — placement graph, validation, execution
- [ ] Starting economic markers (POP / IND / RES) per faction (§5.1–5.3) — *not placed by setup*
- [ ] Peace/War markers, all factions "at Peace" (§15.1)
- [ ] Draw starting hands of Action cards (Axis double) (§5.0)
- [ ] Short Game 1939 setup variant (§17.0)
- [ ] Two-player setup variant (§18.0)

---

## New Year (runs once per Game-Year, in Turn Order)

### [ ] 1. Year Start Phase (§7.1) — `Subphase.YearStart`
- [ ] Behavior for Year Start (currently `TTSyncronizationBehavior.NewYear()` only advances the year/season markers)
- [ ] Advance Year marker (§7.11)
- [ ] Economic Victory check — 25 VP at New Year (§16.2)
- [ ] Reshuffle Action & Investment decks (§7.12)
- [ ] Peace Dividends — deal chits to at-Peace factions (§7.13, §16.1)
- [ ] Determine Turn Order (die roll, §7.14) + Active Player marker

### [ ] 2. Production Phase (§7.2) — `Subphase.Production`
- [ ] `ProductionBehavior` + production `IPlayerAction`(s)
- [ ] Determine Production Level = min(IND, POP[, RES]) (§7.21)
- [ ] Production Blockade resolution, War only (§7.211 / §14.23)
- [ ] Spend Production: build unit steps (§7.23)
- [ ] Build new Cadres in Home/Colonial territory (§7.23)
- [ ] Add CV steps to existing units, CV caps (§7.23, §3.12)
- [ ] Build Fortresses (§7.231)
- [ ] Buy Action / Investment cards (§7.24)

### [ ] 3. Government Phase (§7.3) — `Subphase.Government`
- [ ] `GovernmentBehavior` + cardplay `IPlayerAction`(s); ends on 3 consecutive Passes (§7.35)
- [ ] Diplomacy cardplay — Influence on Neutrals, opposing cards cancel (§7.31, §8.0)
- [ ] Industry — play Investment cards ≥ Factory Cost to raise IND; max twice/year (§7.32)
- [ ] Technology — play Tech pairs, Secret Vault (§7.33, §9.0)
- [ ] Intelligence — execute Intel card effects (§7.34, §9.5)
- [ ] Pass (§7.35)
- [ ] Diplomacy Resolution — place/remove Influence, adjust POP/RES (§7.4, §8.0)
- [ ] HandSize Compliance — discard to limit (§7.5)

---

## Each Season (Spring / Summer / Fall, reduced in Winter)

### [ ] 4. Command Phase (§10.1) — `Subphase.ChooseCommandCards`
- [ ] `CommandBehavior` + commit-command `IPlayerAction` (commit card face-down or Pass)
- [ ] Reveal on all-Pass; if no cards committed, advance Season (§10.1)
- [ ] Determine Command Order from Command Priority letters (§10.1)
- [ ] Command Value → number of units movable (§11.0)
- [ ] Emergency Command for wrong-Season cards (§10.11)
- [ ] Winter Command Phase — USSR only, one card, USSR territory (§10.4)

### [ ] 5. Movement Phase (§11.0) — `Subphase.Movement`
- [ ] `MovementBehavior` + move `IPlayerAction` (graph of legal moves, capped by Command Value)
- [ ] Land Movement, stop on Enemy-occupied (§11.1)
- [ ] Sea Movement / Ocean cost / Convoys / Sea Invasions (§11.2–11.221)
- [ ] Air Movement (§11.3)
- [ ] Strategic Movement (double, friendly-only) (§11.4)
- [ ] Engaging / Disengaging, Border Limits, BattleGroups (§11.5–11.53)
- [ ] Aggression / Raids tagging (§11.54–11.55)
- [ ] Declare War / Violation of Neutrality gating before movement (§15.2, §15.4)

### [ ] 6. Combat Phase (§12.0) — `Subphase.Combat`
- [ ] `CombatBehavior` + combat-action `IPlayerAction`(s) (Fire / Retreat per unit)
- [ ] Designate Active Battles; must resolve Aggressions (§12.1)
- [ ] Combat Rounds, unit-table ordering, Defender-first / FirstFire (§12.21, §12.3)
- [ ] Fire: target class, firepower, dice, applying hits (§12.4)
- [ ] Retreats & ReBasing (§12.5, §13.0) — `Subphase.Rebase`
- [ ] Special Land combat: Invasions, Strategic Bombing, Raids (§12.6)
- [ ] Special Sea combat: BattleGroups, Convoys, Sub Escape, Carrier strikes (§12.7)
- [ ] Battle-ending rules, 3-way battles (§12.22–12.24)

### [ ] 7. Supply Phase (§14.0) — War factions only
- [ ] `SupplyBehavior`
- [ ] Supply Check — trace Supply Lines, Unsupplied step loss (§14.1–14.11)
- [ ] Blockade declaration (Summer only) — Trade Routes, Blockades, Med Blockade (§14.2–14.24)
- [ ] Winter Supply Phase — USSR units only (§10.4)

---

## AI support

- [x] `AiBase` action-graph search + `AiRandom` (works for any phase that exposes `IPlayerAction`s)
- [ ] Per-phase heuristics/scores once each phase's actions exist (Production, Government, Movement, Combat, …)

---

## Notes

- Every phase above (except Setup) currently has **no behavior** and its
  `Subphase` enum value is defined but unused. The matching `GameBehavior`s need
  creating and registering via `ServerGameState.SetEnabledGameBehaviors(...)`.
- Most phases need at least one `IPlayerAction` implementation so both the human
  UI and `AiBase` can drive them; `InitialPlacementAction` is the reference example.
- Two-player (§18.0) and Short Game (§17.0) variants are listed under Setup but
  also ripple into Year Start, Government, and the Season phases.
