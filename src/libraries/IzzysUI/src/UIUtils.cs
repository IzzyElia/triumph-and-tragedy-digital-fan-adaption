using Godot;

namespace TT2026.IzzysUI;

public static class UIUtils
{
    
    public static bool NodesOverlap(Control controlA, Control controlB)
    {
        return controlA.GetGlobalRect().Intersects(controlB.GetGlobalRect());
    }
}