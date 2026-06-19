# Triumph & Tragedy — Phases Overview

This document lists every phase of a Game-Year in order. Each phase has its own
detailed walkthrough file in this folder.

A full game lasts **10 Game-Years**. Each Game-Year consists of a **New Year**
procedure followed by **four Seasons** of play (Spring, Summer, Fall, Winter).

Before the first Game-Year, there is a one-time **Setup** step (not a phase, but
part of the game flow) — see `00 - Setup.md`.

```
SETUP  (§5.0, once before the game)  → 00 - Setup.md
  │  deploy starting units · set POP/IND/RES · all factions "at Peace" · draw hands
  ▼
GAME-YEAR  (repeats 10 times)
│
├── NEW YEAR  (§7.0)
│   ├── 1. Year Start Phase      (§7.1)   → 01 - Year Start Phase.md
│   ├── 2. Production Phase      (§7.2)   → 02 - Production Phase.md
│   └── 3. Government Phase      (§7.3)   → 03 - Government Phase.md
│
├── SPRING  (Season, §10.0)
│   ├── 4. Command Phase         (§10.1)  → 04 - Command Phase.md
│   ├──    Player Turns (Turn Order):
│   │       ├── 5. Movement Phase (§11.0) → 05 - Movement Phase.md
│   │       └── 6. Combat Phase   (§12.0) → 06 - Combat Phase.md
│   └── 7. Supply Phase          (§14.0)  → 07 - Supply Phase.md   (War only)
│
├── SUMMER  (same structure as Spring)
│   └── Supply Phase also performs the Blockade Check (§14.22)
│
├── FALL  (same structure as Spring)
│
└── WINTER  (USSR only)
    ├── Command Phase   (USSR commits one card)
    ├── USSR Player Turn (Movement + Combat, USSR territory only)
    └── Supply Phase     (USSR units only)
```

## Pre-Game Step

| Step | Rule | When it happens | Who acts |
|------|------|-----------------|----------|
| **Setup** | §5.0 | Once, before the first Game-Year | All factions |

## The Seven Phases

| # | Phase | Rule | When it happens | Who acts |
|---|-------|------|-----------------|----------|
| 1 | **Year Start Phase** | §7.1 | Start of each Game-Year | All factions |
| 2 | **Production Phase** | §7.2 | After Year Start | Each faction, in Turn Order |
| 3 | **Government Phase** | §7.3 | After Production | Each faction, in Turn Order |
| 4 | **Command Phase** | §10.1 | Start of each Season | All factions (commit a Command card or Pass) |
| 5 | **Movement Phase** | §11.0 | During each Player Turn | The Active Player |
| 6 | **Combat Phase** | §12.0 | During each Player Turn, after Movement | The Active Player |
| 7 | **Supply Phase** | §14.0 | End of each Season | Factions **at War** only |

## Key structural notes

- **New Year phases (1–3)** happen once per Game-Year and run in **Turn Order**
  (the order the players take turns this year, set by a die roll in §7.14).
- **Season phases (4–7)** repeat for each of the four Seasons. A Season's
  Movement + Combat happen inside **Player Turns**, taken in **Command Order**
  (set by the Command cards committed in the Command Phase, §10.1).
- The **Supply Phase** only matters for factions **at War**. Factions at Peace
  skip it entirely. In **Summer**, the Supply Phase also includes the
  **Blockade Check** (§14.22).
- **Winter** is a reduced Season in which **only the USSR** acts, and only
  inside USSR territory.

## Phase ordering: Turn Order vs. Command Order

There are two different orderings used during a Game-Year:

- **Turn Order** (§7.14): set at Year Start by a die roll. Governs the New Year
  phases (Production, Government). Also the tiebreaker/sequence for Supply,
  Diplomacy Resolution, etc.
- **Command Order** (§10.1): set each Season by the Command Priority letter on
  the Command cards each player secretly committed. Governs the order of Player
  Turns (Movement + Combat) within that Season.

See the individual phase files for the full rules of each.
