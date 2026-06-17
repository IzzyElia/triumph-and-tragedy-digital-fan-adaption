using Godot;

namespace TT2026.libraries.IzzysUI.Tooltips;

public partial class ContextWindowLinkController : Control
{
    [Export] private Button _button;
    [Export] private Label _label;
    
    private ILinkable _target;
    private ContextWindowController _source;
    public override void _Ready()
    {
        base._Ready();
        _button.Pressed += FollowLink;
    }

    public void Setup(ContextWindowController source, ILinkable target)
    {
        _target = target;
        _source = source;
        _label.Text = target.Name;
    }

    void FollowLink()
    {
        _source.SetOpenLink(_target);
    }
}