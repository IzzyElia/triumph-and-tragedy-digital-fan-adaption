using Godot;

namespace TT2026.IzzysUI;

public abstract class UIStat : IUIStat
{
    public abstract string Label { get; }
    public abstract string DisplayedValue { get; }
    public abstract float ValuePercentage { get; }
    public Color HeaderColor { get; protected set; }
    public Color LabelColor { get; protected set; }
    public Color BarColorEmpty { get; protected set; }
    public Color BarColorFull { get; protected set; }

    protected UIStat(Color headerColor, Color labelColor, Color barColorEmpty, Color barColorFull)
    {
        this.HeaderColor = headerColor;
        this.LabelColor = labelColor;
        this.BarColorEmpty = barColorEmpty;
        this.BarColorFull = barColorFull;
    }
}