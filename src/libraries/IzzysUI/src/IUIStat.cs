using Godot;
using TT2026.IzzysUI.Tooltips;

namespace TT2026.IzzysUI;

public interface IUIStat : IUIInteraction
{
    public string DisplayedValue { get; }
    public float ValuePercentage { get; }
    public Color HeaderColor { get; }
    public Color LabelColor { get; }
    public Color BarColorEmpty { get; }
    public Color BarColorFull { get; }
}