using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Principal;
using System.Text.Json;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;

namespace TT2026.Libraries.NetworkedBoardGameEntitySystem.Saving;

public static class GameStateSaver
{
    public static object SaveFileMutex = new object();
    private struct EntityData (Type type)
    {
        public string Type { get; set; } = type.FullName;
        public Dictionary<string, List<DataHistoryFrame>> Data { get; set; } = new();
    }
    public static void SaveToFile(string fileName, string json)
    {
        lock (SaveFileMutex)
        {
            string fullPath = Path.Join(Directory.GetCurrentDirectory(), fileName);
            File.WriteAllText(fullPath, json);
        }
    }
    
    public static string LoadFromFile(string fileName)
    {
        lock (SaveFileMutex)
        {
            return File.ReadAllText(Path.Join(Directory.GetCurrentDirectory(), fileName));
        }
    }

    public static string SerializeGameStae(GameState gameState, bool prettyPrint = false)
    {
        lock (gameState.NetworkManager.Mutex)
        {
            // <entity id, (entity type, dict<__SharedState field, serialized data value>)>
            Dictionary<string, EntityData> data = new();
            foreach (GameEntity entity in gameState.EntitiesById.Values)
            {
                var entityData = new EntityData(entity.GetType());
                data.Add(entity.ID.ToString(CultureInfo.InvariantCulture), entityData);
                foreach ((string key, EntityGameData dataObj) in entity.__SyncedData)
                {
                    entityData.Data.Add(key, dataObj.__HistoryFrames);
                }
            }

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions()
            {
                WriteIndented = prettyPrint
            });
            return json;
        }
    }

    /// <summary>
    /// Creates a new game state for a server by deserializing JSON (json that was generated using SerializeGameState)
    /// </summary>
    /// <param name="server">The Server object the GameState is meant for</param>
    /// <param name="json">The JSON being deserialized</param>
    /// <returns>A new GameState created from deserialized JSON</returns>
    /// <exception cref="InvalidOperationException">Thrown when</exception>
    public static ServerGameState CreateGamestateFromJson(Server server, string json)
    {
        lock (server.Mutex)
        {
            ServerGameState gameState = new ServerGameState(server);
            int mostRecentStep = 0;
            Dictionary<string, EntityData> data = JsonSerializer.Deserialize<Dictionary<string, EntityData>>(json);
            foreach ((string sId, EntityData entityData) in data)
            {
                if (!int.TryParse(sId, CultureInfo.InvariantCulture, out int id)) throw new InvalidOperationException();
                Type type;
                string sType = entityData.Type;
                try
                {
                    type = GameState.KnownEntityTypes[sType];
                }
                catch (KeyNotFoundException)
                {
                    throw new InvalidOperationException();
                }

                GameEntity entity = gameState.InstantiateGameEntity(type, id);
                if (entity is GameBehavior behavior)
                    gameState.GameBehaviors.Add(behavior.GetType(), behavior);
                foreach ((string key, List<DataHistoryFrame> historyFrames) in entityData.Data)
                {
                    foreach (var historyFrame in historyFrames)
                    {
                        try
                        {
                            if (!entity.__SyncedData[key].ValidateData(historyFrame.Data)) throw new InvalidOperationException();
                            entity.__SyncedData[key].ForceSetValue(historyFrame.Data, historyFrame.GameStepID, push: false);
                            if (historyFrame.GameStepID > mostRecentStep) mostRecentStep = historyFrame.GameStepID;
                        }
                        catch (KeyNotFoundException)
                        {
                            Logger.Log($"Unable to find {sType}.{key}. It may just be depreciated");
                        }
                    }
                }
            }
            gameState.GameStepID = mostRecentStep;
            return gameState;
        }
    }
}