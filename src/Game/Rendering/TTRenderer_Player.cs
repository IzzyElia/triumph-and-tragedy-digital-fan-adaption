using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TT2026.Game.Actions;
using TT2026.Game.AI;
using TT2026.Game.Entities;
using TT2026.Game.Rendering.BoardObjects;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.IzzysUI;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Networking.PacketTypes;

namespace TT2026.Game.Rendering;

/// <summary>
/// A variant of the Renderer intended for normal game play
/// </summary>
public partial class TTRenderer_Player : TTRenderer
{
    private IPlayerAction contemplatedAction = null;
    private List<IPlayerAction> potentialActions = new List<IPlayerAction>();
    private Dictionary<int, UnitBoardObject> _uncommittedPlacementRenderers = new();
    

    public override void Initialize()
    {
        base.Initialize();
        RecalculatePotentialActions();
    }

    protected override void OnTileClicked(ICoordinate2d tileCoordinate, TileClickMetadata input)
    {
        base.OnTileClicked(tileCoordinate, input);
        int boardSpaceId = GameState.GetGameBehavior<TileOwnershipBehavior>().GetOwnerOfTile(tileCoordinate);
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
        if (boardSpace is null) return;
        
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        foreach (var potentialAction in potentialActions)
        {
            if (potentialAction is IPlacementAction placementAction)
            {
                OnPlacementAction(placementAction, boardSpace, ref interactions);
            }
        }
        
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            header:"Actions",
            interactions: interactions));
    }


    
    
    
    // Action types
    public override void UnitRefresh()
    {
        base.UnitRefresh();
        if (contemplatedAction is InitialPlacementAction placementAction)
        {
            MatchRendererDictToArray(
                array: placementAction.Placements, 
                dict: ref _uncommittedPlacementRenderers, 
                instantiator: () => UnitBoardObject.Prefab.Instantiate<UnitBoardObject>()
                );

            foreach ((int index, UnitBoardObject renderer) in _uncommittedPlacementRenderers)
            {
                var placement = placementAction.Placements[index];
                PlaceUnitRendererOnSpace(placement.BoardSpaceId, renderer);
            }
        }
        else
        {
            MatchRendererDictToArray(
                array: Array.Empty<InitialPlacementAction.Placement>(),
                dict: ref _uncommittedPlacementRenderers, 
                instantiator: () => UnitBoardObject.Prefab.Instantiate<UnitBoardObject>()
            );
        }
    }

    protected override void RecalculatePotentialActions()
    {
        lock (Client.Mutex)
        {
            HighlightSpaces.Clear();
            potentialActions.Clear();
            if (GameState is null) return;
            
            if (contemplatedAction?.Validate(GameState) == ActionValidationResult.Illegal)
            {
                contemplatedAction = null;
            }
            
            foreach (var potentialAction in GameState.GetPlayerActions(GameState.PlayerId, contemplatedAction))
            {
                potentialActions.Add(potentialAction);
                foreach (var entityId in potentialAction.HighlightEntities(GameState))
                    HighlightSpaces.Add(entityId);
            }
        }

        RefreshesNeeded.Add(RefreshType.Tiles);
    }
    public void ContemplateAction(IPlayerAction action)
        {
            if (action?.Validate(GameState) == ActionValidationResult.Illegal)
            {
                if (action.From.Validate(GameState) == ActionValidationResult.Illegal)
                {
                    contemplatedAction = null;
                    RecalculatePotentialActions();
                    return;
                }
                else
                {
                    contemplatedAction = action.From;
                    return;
                }
            }
            contemplatedAction = action;
            RecalculatePotentialActions();
            RefreshesNeeded.Add(RefreshType.Units);
        }
    private void OnPlacementAction(IPlacementAction placementAction, BoardSpace boardSpace, ref List<IUIInteraction> interactions)
    {
        if (placementAction.BoardSpaceId == boardSpace.ID)
        {
            interactions.Add(new SimpleUIAction(placementAction.StepDescription(GameState), () =>
            {
                ContemplateAction(placementAction);
            }));
        }
    }

    
    // General UI
    public void OpenMenuContextWindow()
    {
        if (Client.ConnectedServer is null) { OnNotConnected(); return; }
        
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        interactions.Add(new SimpleUIAction("Start Game", () =>
        {
            TTUtils.StartGameDefault(GameState);
        }));
        interactions.Add(new SimpleUIActionAsync($"Confirm Action ({contemplatedAction?.GetType().Name})", async () =>
        {
            try
            {
                var response = await Client.SendRequestAwaitCallback(Client.ConnectedServer, new PlayerActionPacket(contemplatedAction));
                if (response.Error == NetworkResponseError.None)
                {
                    contemplatedAction = null;
                    potentialActions.Clear();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            return null;
        }));
        interactions.Add(new SimpleUIAction($"Pick Random Action", () =>
        {
            try
            {
                IPlayerAction action = new AiRandom().PickAction(GameState, GameState.PlayerId, int.MaxValue);
                GameState.AttemptAction(action);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }));
        
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            source: null,
            header: "",
            text: $"Game Step: {GameState.GameStepID}",
            interactions: interactions));
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mb)
        {
            if (!mb.Pressed && mb.ButtonIndex == MouseButton.Right)
            {
                if (IzzysUIController.IsContextWindowOpen)
                {
                    IzzysUIController.CloseActiveContextWindow();
                }
                else
                {
                    OpenMenuContextWindow();
                }
            }
        }

        if (@event is InputEventKey key)
        {
            int setPlayerId = -1;
            if (key.Pressed && key.Keycode == Key.Key1)
                setPlayerId = 0;
            else if (key.Pressed && key.Keycode == Key.Key2)
                setPlayerId = 1;
            else if (key.Pressed && key.Keycode == Key.Key3)
                setPlayerId = 2;
            else if  (key.Pressed && key.Keycode == Key.Key4)
                setPlayerId = 3;
            else if (key.Pressed && key.Keycode == Key.Key5)
                setPlayerId = 4;

            if (setPlayerId != -1)
            {
                var factions = GameState.GetEntitiesOfType<Faction>().ToArray();
                if (setPlayerId < factions.Length)
                {
                    Logger.Log($"Setting client to player {factions[setPlayerId].ID}");
                    GameState.TrySetPlayerSlot(factions[setPlayerId].ID);
                }
            }
        }
    }
}