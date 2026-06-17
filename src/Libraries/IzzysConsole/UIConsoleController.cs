using Godot;
using TT2026.libraries.IzzysConsole.API;

namespace TT2026.libraries.IzzysConsole;

public partial class UIConsoleController : Control, IConsoleController
{
    [Export] private Control _mainPanel;
    [Export] private TextEdit _textEdit;
    [Export] private Label _logDisplay;
    
    public override void _EnterTree()
    {
        base._EnterTree();
        ConsoleManager.RegisterConsole(this);
        this.Visible = false;
        SetFocusModeRecursively(_mainPanel, _mainPanel.Visible ? FocusModeEnum.Click : FocusModeEnum.None);
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        ConsoleManager.UnregisterConsole(this);
    }

    private bool _firstFrameOfPress = true;
    private bool _firstFrameOfEnterPress = true;
    public override void _Process(double delta)
    {
        base._Process(delta);
        if (Input.IsKeyPressed(Key.Quoteleft) || Input.IsKeyPressed(Key.Apostrophe))
        {
            if (_firstFrameOfPress)
            {
                _firstFrameOfPress = false;
                GD.Print("toggling console");
                this.Visible = !this.Visible;
                SetFocusModeRecursively(_mainPanel, this.Visible ? FocusModeEnum.Click : FocusModeEnum.None);
            }
        }
        else
        {
            _firstFrameOfPress = true;
        }

        if (IsActive)
        {
            if (Input.IsKeyPressed(Key.Enter))
            {
                if (_firstFrameOfEnterPress)
                {
                    ConsoleManager.TryExecuteCommand(_textEdit.Text);
                    _textEdit.Text = ""; 
                }
                _firstFrameOfEnterPress = false;

            }
            else
            {
                _firstFrameOfEnterPress = true;
            }
        }
    }

    public bool IsActive => _mainPanel.Visible;
    public bool IsFocused => IsActive;
    public void OnConsoleLogChanged(string text)
    {
        _logDisplay.Text = text;
    }
    
    private static void SetFocusModeRecursively(Control target, Control.FocusModeEnum focusMode)
    {
        target.SetFocusMode(focusMode);
        foreach (var node in target.GetChildren())
        {
            if (node is Control control) SetFocusModeRecursively(control, focusMode);
        }
    }
}