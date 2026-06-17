namespace TT2026.libraries.IzzysUI.Tooltips;

/// <summary>
/// Represents a link to another tooltip
/// </summary>
public interface ITooltipLink : IUIInteraction
{
    public ILinkable Target { get; }
}