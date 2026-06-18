using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using TT2026.libraries.IzzysUI.ObjectSelector;
using TT2026.Libraries.IzzysUI.Popups;
using TT2026.libraries.IzzysUI.Tooltips;

namespace TT2026.libraries.IzzysUI;

/// <summary>
/// Usage:
///     Create a node of this type in the scene
///     Set the HoveredTooltipObject property to tell the controller what is being hovered over for tooltip purposes
///     Call OpenContextWindow() to open a contextual UI window from a target object
///     Call CloseActiveContextWindow() to close the open window
/// </summary>
public partial class IzzysUIController : Node
{
    static IzzysUIController _instance;
    
    [Export] private Control _contextWindowParentControl;
    private Control _contextWindowContainer;
    
    private const double _tooltipTimeToTrigger = 0.4d;
    private static double _tooltipTimer;
    private static double _tooltipExitTimer;
    private static ITooltipCreator _prevHoveredTooltipObject;
    private static ContextWindowController _activeTooltip;
    private static ContextWindowController _activeContextWindow;
    private static PopupController _activePopup;
    private static ITooltipCreator _activeTooltipSource;
    public static ITooltipCreator HoveredTooltipObject = null;

    public static ContextWindowController ContextWindowUnderMouse;
    public static bool IsContextWindowOpen => _activeContextWindow is not null;

    public override void _EnterTree()
    {
        _instance = this;
        _contextWindowContainer = new HFlowContainer();
        _contextWindowContainer.Name = "Context Window Container";
    }

    public override void _Ready()
    {
        _contextWindowParentControl.CallDeferred("add_child", _contextWindowContainer);
    }

    private static void OnUpdate_TooltipControl(double delta)
    {
        if (ContextWindowUnderMouse is null && HoveredTooltipObject != _activeTooltipSource) _tooltipExitTimer += delta;
        else _tooltipExitTimer = 0;
        
        if (HoveredTooltipObject is not null && HoveredTooltipObject == _prevHoveredTooltipObject)
        {
            if (_activeTooltipSource == HoveredTooltipObject) _tooltipExitTimer = 0;
            _tooltipTimer += delta;
            if (_tooltipTimer >= _tooltipTimeToTrigger && _activeTooltip is null && _activeContextWindow is null)
            {
                ContextWindowInfo tooltipInfo = HoveredTooltipObject.GetTooltip();
                if (tooltipInfo is not null)
                {
                    _activeTooltip = ContextWindowController.Instantiate(tooltipInfo, _instance.GetViewport().GetMousePosition());
                    _activeTooltipSource = HoveredTooltipObject;
                    _tooltipExitTimer = 0;
                }
            }
        }
        
        if (_tooltipExitTimer >= _tooltipTimeToTrigger)
        {
            CloseActiveTooltip();
        }

        _prevHoveredTooltipObject = HoveredTooltipObject;
    }

    private static void CloseActiveTooltip()
    {
        if (_activeTooltip is null) return;
        _activeTooltip.DoClose();
        _activeTooltip = null;
        _activeTooltipSource = null;
    }

    public static void RefreshUI()
    {
        _activeTooltip?.Refresh();
        _activeContextWindow?.Refresh();
        _activePopup?.Refresh();
        foreach (var displayWindow in _activeDisplayWindowByCategory.Values)
        {
            displayWindow.Refresh();
        }
    }


    private void OnUpdate_ContextWindowControl(double delta)
    {
        
    }
    public static void OpenContextWindow(ContextWindowInfo contextWindowInfo, Vector2? position = null)
    {
        CloseActiveContextWindow();
        CloseActiveTooltip();
        
        if (position is null) position = _instance.GetViewport().GetMousePosition();
        _activeContextWindow = ContextWindowController.Instantiate(contextWindowInfo, position.Value, parentNode:_instance._contextWindowContainer);
        _instance._contextWindowContainer.GlobalPosition = _activeContextWindow.GlobalPosition;
        _instance._contextWindowContainer.AnchorRight = 1f;
        _instance._contextWindowContainer.OffsetRight = 0f;
    }
    public static void CloseActiveContextWindow()
    {
        if (_activeContextWindow is null) return;
        _activeContextWindow.DoClose();
        _activeContextWindow = null;
    }

    public static async Task<object> OpenPopupAndGetResult(PopupInfo popupInfo)
    {
        TaskCompletionSource<object> popupTaskSource = new();
        PopupController popup = popupInfo.InstantiateController(
            parent: _instance._contextWindowParentControl, 
            tcs: popupTaskSource);
        object result = await popupTaskSource.Task;
        return result;
    }

    public static void CloseActivePopup()
    {
        if (_activePopup is null) return;
        _activePopup.Close();
        _activePopup = null;
    }

    private static Dictionary<UIWindowCategory, DisplayController> _activeDisplayWindowByCategory =
        new Dictionary<UIWindowCategory, DisplayController>();
    
    /*
    public override void _UnhandledInput(InputEvent @event)
    {
        base._UnhandledInput(@event);
        if (@event is InputEventMouseButton mb)
        {
            if (mb.ButtonIndex == MouseButton.Right)
            {
                CloseActiveContextWindow();
                CloseTileObjectSelectorIfOpen();
                foreach (var window in _activeDisplayWindowByCategory.Values)
                {
                    window.DoClose();
                }
            }
        }
    }
    */

    public static bool DebugHeld => true;

    private static UIObjectSelector _activeTileObjectSelector = null;

    public static void CloseTileObjectSelectorIfOpen()
    {
        if (_activeTileObjectSelector is not null)
        {
            _activeTileObjectSelector.QueueFree();
            _activeTileObjectSelector = null;
        }
    }
}

public enum UIWindowCategory
{
    IndependantPopup,
    CenterPopup,
    BottomWindow
}