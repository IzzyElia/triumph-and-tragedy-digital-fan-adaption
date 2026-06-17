using Godot;

namespace TT2026.libraries.IzzysUI;

// ReSharper disable once InconsistentNaming
public partial class UIActionController : Control
{
    [Export] private Button _button;
    [Export] private Label _label;
    
    private IAction _action;
    private DisplayController _contextWindowController;
    public void SetAction(IAction action)
    {
        if (_action is not null)
        {
            _button.Pressed -= ExecuteAction;
        }
        _action = action;
        _button.Pressed += ExecuteAction;
        _button.Disabled = !action.IsActionAllowed();
        _label.Text = action.Label;
    }

    private void ExecuteAction()
    {
        _action.ExecuteAction();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}