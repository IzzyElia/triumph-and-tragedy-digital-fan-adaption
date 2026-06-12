using System;
using Godot;

namespace TT2026.IzzysUI;


public partial class UIGenericLink : Control
{
    [Export] private Button _button;
    [Export] private Label _label;
    private Action<object> _onClick;
    private object _parameter;
    public override void _Ready()
    {
        base._Ready();
        _button.Pressed += FollowLink;
    }

    /// <param name="onClick">The method to invoke when the button is clicked</param>
    /// <param name="target">The parameter that will be passed to the method</param>
    /// <param name="label">The text on the button label</param>
    public void Setup(Action<object> onClick, object parameter, string label)
    {
        _parameter = parameter;
        _onClick = onClick;
        _label.Text = label;
    }

    void FollowLink()
    {
        _onClick.Invoke(_parameter);
    }
}