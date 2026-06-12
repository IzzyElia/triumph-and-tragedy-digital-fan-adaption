namespace TT2026.IzzysUI.Tooltips;

public class Link<T> : ITooltipLink where T : ILinkable
{
    private T _target;
    public string Label => _target.Name;
    public ILinkable Target => _target;

    public Link(T target)
    {
        _target = target;
    }
}