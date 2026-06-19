using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using TT2026.Game.Actions;
using TT2026.Game.Entities;
using TT2026.libraries.Izzy.Geometry;
using TT2026.libraries.IzzysUI;
using TT2026.libraries.IzzysUI.Tooltips;
using TT2026.Libraries.NetworkedBoardGameEntitySystem;
using TT2026.Libraries.NetworkedBoardGameEntitySystem.Actions;

namespace TT2026.Game.Rendering;

/// <summary>
/// A variant of the Renderer intended for normal game play
/// </summary>
public partial class TTRenderer_Player : TTRenderer
{
    private IPlayerAction contemplatedAction = null;
    private List<IPlayerAction> potentialActions = new List<IPlayerAction>();
    protected override void OnTileClicked(ICoordinate2d tileCoordinate, TileClickMetadata input)
    {
        base.OnTileClicked(tileCoordinate, input);
        int boardSpaceId = GameState.GetGameBehavior<TileOwnershipBehavior>().GetOwnerOfTile(tileCoordinate);
        BoardSpace boardSpace = GameState.GetEntity<BoardSpace>(boardSpaceId);
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        foreach (var potentialAction in potentialActions)
        {
            if (potentialAction is IPlacementAction placementAction)
            {
                OnPlacementAction(placementAction, boardSpace, ref interactions);
            }
        }
    }

    public void ContemplateAction(IPlayerAction action)
    {
        if (action.Validate(GameState) == ActionValidationResult.Illegal)
        {
            if (action.From.Validate(GameState) == ActionValidationResult.Illegal)
            {
                contemplatedAction = null;
                RecalculatePotentialActions();
            }
        }
        contemplatedAction = action;
        RecalculatePotentialActions();
    }

    private void RecalculatePotentialActions()
    {
        lock (potentialActions)
        {
            potentialActions.Clear();
            foreach (var potentialAction in GameState.GetPlayerActions(GameState.PlayerId, contemplatedAction))
            {
                potentialActions.Add(potentialAction);
            }
        }
    }
    
    
    
    // Action types
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

    public void OpenMenuContextWindow()
    {
        List<IUIInteraction> interactions = new List<IUIInteraction>();
        interactions.Add(new SimpleUIAction("Start Game", () =>
        {
            throw new NotImplementedException();
        }));
        
        IzzysUIController.OpenContextWindow(new ContextWindowInfo(
            source: null,
            header: "",
            text: "",
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
    }
}