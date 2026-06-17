using Godot;

namespace TT2026.libraries.IzzysUI;

public abstract partial class DisplayController : Control
{
    public abstract UIWindowCategory CategoryTag { get; }

    public void DoClose()
    {
        Close();
    }
    protected abstract void Close();
    public abstract void Refresh();
}