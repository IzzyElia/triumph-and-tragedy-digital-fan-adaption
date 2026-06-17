using Godot;

namespace TT2026.libraries.IzzysUI.ObjectSelector;

public partial class UIObjectSelectionLink : Control
{
    [Export] private Button _button;
    [Export] private Label _label;
    
    private ISelectable _target;
    public override void _Ready()
    {
        base._Ready();
        _button.Pressed += FollowLink;
    }

    public void Setup(ISelectable target)
    {
        _target = target;
        _label.Text = target.Name;
    }

    void FollowLink()
    {
        _target.OnSelected(IzzysUIController.DebugHeld);
        IzzysUIController.CloseTileObjectSelectorIfOpen();
    }
}