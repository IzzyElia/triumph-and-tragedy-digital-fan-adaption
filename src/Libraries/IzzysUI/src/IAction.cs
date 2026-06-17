
using TT2026.libraries.IzzysUI.Tooltips;

namespace TT2026.libraries.IzzysUI;

/// <summary>
/// 
/// </summary>
public interface IAction : IUIInteraction
{
    public void ExecuteAction();
    public bool IsActionDisplayed();
    public bool IsActionAllowed();
}