using Godot;

namespace TT2026.IzzysUI;

public partial class UIStatDisplayController : Control
{
    [Export] private Label _headerLabel;
    [Export] private Label _valueLabel;
    [Export] private ProgressBar _progressBar;
    
    public void SetValue(string displayedValue, float percentageValue = 0)
    {
        _valueLabel.Text = displayedValue;
        _progressBar.Value = percentageValue;
    }
}