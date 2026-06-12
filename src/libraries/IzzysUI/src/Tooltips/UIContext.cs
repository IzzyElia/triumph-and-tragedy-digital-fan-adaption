namespace TT2026.IzzysUI.Tooltips;

public class UIContext
{
    public IContextWindowCreator Source { get; private set; }
    public UIContext(IContextWindowCreator source) : base()
    {
        this.Source = source;
    }
}