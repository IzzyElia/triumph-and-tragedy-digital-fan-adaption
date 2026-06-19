using System;
using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Behaviors;

/// <summary>
/// This game behavior is in charge of tracking the current game phase and player
/// ordering. Other behaviors can reference it to determine what phase the
/// game is in and what player should be active.
/// </summary>
public class TTSyncronizationBehavior : TTGameBehavior
{
    public SyncedInt CurrentYear;
    public SyncedInt CurrentSeason;
    public SyncedInt CurrentSubphase; // May be either a NewYearSubphase or a SeasonSubphase
    public SyncedInt PhasingFaction; // By faction ID

    public TTSyncronizationBehavior()
    {
        CurrentYear = new SyncedInt(this, nameof(CurrentYear), -1);
        CurrentSeason = new SyncedInt(this, nameof(CurrentSeason), (int)Season.Undefined);
        CurrentSubphase = new SyncedInt(this, nameof(CurrentSubphase), (int)Subphase.Undefined);
        PhasingFaction = new SyncedInt(this, nameof(PhasingFaction), -1);
    }
    
    public TTPhaseData GetPhaseData()
    {
        if (CurrentSeason.Value == (int)Season.Undefined 
            || CurrentSubphase.Value == (int)Subphase.Undefined 
            || CurrentYear.Value == -1)
            throw new InvalidOperationException($"Phase hasn't been set or was reset to its default value");
        return new TTPhaseData()
        {
            Year = CurrentYear.Value,
            Season = (Season)CurrentSeason.Value,
            Subphase = (Subphase)CurrentSubphase.Value,
            PhasingFaction = PhasingFaction.Value,
        };
    }


    public override void OnGameStart(IGameStartInfo gameStartInfo)
    {
        CurrentYear.Value = 1935;
        CurrentSeason.Value = (int)Season.Setup;
        CurrentSubphase.Value = (int)Subphase.NotApplicable;
        PhasingFaction.Value = -1;
        CommitState();
    }

    public override void OnPhaseTickerAdvancing()
    {
        
    }

    public override IEnumerable<IPlayerAction> GetPotentialActions(int factionId)
    {
        yield break;
    }
}

public struct TTPhaseData
{
    public int Year { get; set; }
    public Season Season { get; set; }
    public Subphase Subphase { get; set; }
    public int PhasingFaction { get; set; }
}

public enum Season
{
    Undefined,
    Setup,
    NewYear,
    Spring,
    Summer,
    Fall,
    Winter,
}

public enum Subphase
{
    // Special
    Undefined,
    NotApplicable,
    
    // New Year subphases
    YearStart, // Automatic checks and changes. Assign turn order
    Production, // In turn order (maybe simultaneous option for digital??), choose production
    Government, // Take turns playing industry or diplomacy cards until all players have passed
    
    // Season subphases
    ChooseCommandCards, // All players simultaneously choose a command card
    Movement, // The phasing player commits moves, then goes to combat
    Combat, // Resolve combats
    Rebase, // Rebase units if available or required
}