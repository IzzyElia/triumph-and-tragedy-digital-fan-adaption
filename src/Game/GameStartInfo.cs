using System;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game;

public struct GameStartInfo : ISyncable, IGameStartInfo
{
    public struct FactionInfo
    {
        public int Leader;
        public int[] Allies;
        public Guid OwnedByClient;
    }
    
    public FactionInfo[] Factions;
}