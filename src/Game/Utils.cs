using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TT2026.Game.Definitions;
using TT2026.Game.Rendering;
using TT2026.libraries.IzzysUI;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.libraries.NetworkedBoardGameEntitySystem;
using TT2026.libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Saving;
using TT2026.libraries.NetworkedBoardGameEntitySystem.SyncedDataTypes;

namespace TT2026.Game;

public static class TTUtils
{
    public static void Quickload(Client client, Action onCompletion = null)
    {
        string json = GameStateSaver.LoadFromFile("scenarios/quicksave.json");
        List<string> chunks = new List<string>();
        foreach (var chunk in Utils.ChunkString(json, 1100))
            chunks.Add(chunk);
        
        for (int i = 0; i < chunks.Count; i++)
        {
            string chunk = chunks[i];
            client.SendRequest(client.ConnectedServer, new ImportGameStatePacket(
                part: i,
                numParts: chunks.Count,
                json: chunk,
                editorAuthKey: null
            ));
        }
        onCompletion?.Invoke();
    }
    public static void StartGameDefault(ClientGameState gameState)
    {
        gameState.Client.SendRequest(
            gameState.Client.ConnectedServer, 
            new StartGamePacket(new GameStartInfo()
            {
                Factions = [
                    new GameStartInfo.FactionInfo(
                        gameState:gameState,
                        name:"Fascists",
                        leader:"Germany",
                        allies:["Italy"],
                        ownedByClient:Guid.Empty),
                    new GameStartInfo.FactionInfo(
                        gameState:gameState,
                        name:"Capitalists",
                        leader:"Britain",
                        allies:["France"],
                        ownedByClient:Guid.Empty),
                    new GameStartInfo.FactionInfo(
                        gameState:gameState,
                        name:"Communists",
                        leader:"USSR",
                        allies:[],
                        ownedByClient:Guid.Empty),
                ]
            }));
    }
    public static T SearchEntity<T>(GameState gameState, string query, Func<T, string> getName, T fallback = null) where T : GameEntity
    {
        query = query.Trim().ToLowerInvariant();
        (int entityId, int matchScore) bestMatch = (-1, 1);
        foreach (var entity in gameState.GetEntitiesOfType<T>())
        {
            int matchScore = 0;
            string name = getName(entity)?.Trim().ToLowerInvariant();
            if (name is null) continue;
            for (int i = 0; i < query.Length; i++)
            {
                if (i >= name.Length) matchScore--;
                else if (query[i] == name[i]) matchScore++;
                else matchScore--;
            }
            if (matchScore > bestMatch.matchScore) bestMatch = (entity.ID, matchScore);
        }
        
        if (bestMatch.entityId == -1) return fallback;
        return gameState.GetEntity<T>(bestMatch.entityId);
    }
    
    public async static Task<T> PopupSearchEntity<T>(GameState gameState, string popupHeader, Func<T, string> getName) where T : GameEntity
    {
        object popupResponse = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
        {
            Header = popupHeader,
            PopupType = PopupType.Text
        });
        var response = (string)popupResponse;
        if (response is null || response.Length == 0) return null;
        response = response.Trim().ToLowerInvariant();
        return SearchEntity(gameState, response, getName);
    }
    
    public static void GenerateEditorActionsForDefinitionEntity(IDefinition entity, TTRenderer_Editor editor, ref List<IUIInteraction> interactions)
    {
        object definition = entity.DefinitionAccessor.__Value;
        foreach (var property in definition.GetType().GetProperties())
        {
            interactions.Add(new SimpleUIActionAsync($"{property.Name} ({property.GetValue(definition)?.ToString()})", async () =>
            {
                try
                {
                    object result = await IzzysUIController.OpenPopupAndGetResult(new PopupInfo()
                    {
                        Header = property.Name,
                        PopupType = PopupType.Text,
                    });
                    string value = ((string)result).Trim();
                    if (property.PropertyType == typeof(string))
                        property.SetValue(definition, value);
                    else if (property.PropertyType == typeof(int))
                        property.SetValue(definition, int.Parse(value));
                    else if (property.PropertyType == typeof(uint))
                        property.SetValue(definition, uint.Parse(value));
                    else if (property.PropertyType == typeof(long))
                        property.SetValue(definition, long.Parse(value));
                    else  if (property.PropertyType == typeof(ulong))
                        property.SetValue(definition, ulong.Parse(value));
                    else if (property.PropertyType == typeof(float))
                        property.SetValue(definition, float.Parse(value));
                    else if (property.PropertyType == typeof(bool))
                        property.SetValue(definition, bool.Parse(value));
                    else throw new NotSupportedException();
                    entity.DefinitionAccessor.__Value = definition;
                    await editor.EditValue(entity.ID, entity.DefinitionAccessor.Key, entity.DefinitionAccessor.SerializeData());
                    
                }
                catch (Exception e)
                {
                    Logger.Error(e.ToString());
                }
                
                return null;
            }));
        }
    }
}