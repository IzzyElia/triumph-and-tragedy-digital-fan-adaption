using Godot;

namespace TT2026.IzzysUI.ObjectSelector;

public partial class UIObjectSelector : Control
{
    [Export] private Control _objectLinkContainer;
    [Export] private PackedScene _objectLinkPrefab;

    public void AddSelectableObject(ISelectable selectableObject)
    {
        UIObjectSelectionLink link = _objectLinkPrefab.Instantiate<UIObjectSelectionLink>();
        link.Setup(selectableObject);
        _objectLinkContainer.AddChild(link);
    }
}