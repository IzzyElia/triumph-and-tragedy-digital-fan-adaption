using Godot;
using TT2026.libraries.IzzysUI.Tooltips;

namespace TT2026.Libraries.IzzysUI.Popups;

public partial class TextPopupController : PopupController
{
    [Export] LineEdit _lineEdit;
    protected override object ReturnResult()
    {
        return _lineEdit.GetText();
    }
}