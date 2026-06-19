using System.Collections.Generic;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game.Behaviors;

/// <summary>
/// This game behavior is in charge of tracking the current game phase and player
/// ordering. Other behaviors can reference it to determine what phase the
/// game is in and what player should be active.
/// </summary>
public class TTSyncronizationBehavior : GameBehavior
{
    public SyncedInt CurrentYear;
    public SyncedInt Season;
    public SyncedInt Subphase; // May be either a NewYearSubphase or a SeasonSubphase
    public SyncedInt PhasingFaction; // By faction ID

    public TTPhaseData GetPhaseData()
    {
        return new TTPhaseData()
        {
            Year = CurrentYear.Value,
            Season = (Season)Season.Value,
            Subphase = (Subphase)Subphase.Value,
            PhasingFaction = PhasingFaction.Value,
        };
    }
    
    
    public override IEnumerable<IPlayerAction> GetPotentialActions()
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
    NotApplicable,
    Setup,
    
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