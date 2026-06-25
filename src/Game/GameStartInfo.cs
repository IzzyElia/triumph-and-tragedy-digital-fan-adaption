using System;
using TT2026.Game.Entities;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game;

public struct GameStartInfo : ISyncable, IGameStartInfo
{
    public struct FactionInfo
    {
        public string Name { get; set; }
        public int Leader { get; set; }
        public int[] Allies { get; set; }
        public Guid OwnedByClient { get; set; }

        public FactionInfo(GameState gameState, string name, string leader, string[] allies, Guid ownedByClient)
        {
            this.Name = name;
            Nation leaderNation = TTUtils.SearchEntity<Nation>(gameState, leader, x => x.Definition.Value.Name);
            if (leaderNation is null) throw new NullReferenceException(nameof(this.Leader));
            Leader = leaderNation.ID;
            Allies = new int[allies.Length];
            for (int i = 0; i < allies.Length; i++)
            {
                Allies[i] = TTUtils.SearchEntity<Nation>(gameState, allies[i], x => x.Definition.Value.Name).ID;
            }
            OwnedByClient = ownedByClient;
        }
        
    }
    
    public FactionInfo[] Factions { get; set; }
}