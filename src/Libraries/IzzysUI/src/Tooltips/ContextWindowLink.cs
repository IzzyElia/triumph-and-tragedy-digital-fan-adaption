namespace TT2026.libraries.IzzysUI.Tooltips;

public class Link<T> : ITooltipLink where T : ILinkable
{
    private T _target;
    public string Label => _target.LinkName;
    public ILinkable Target => _target;

    public Link(T target)
    {
        _target = target;
    }
}

public class Link : ITooltipLink
{
    private ILinkable _target;
    public string Label => _target.LinkName;
    public ILinkable Target => _target;

    public Link(ILinkable target)
    {
        _target = target;
    }
}