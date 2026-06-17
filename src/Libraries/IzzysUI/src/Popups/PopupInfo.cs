using System;
using System.Threading.Tasks;
using Godot;
using TT2026.libraries.IzzysUI.Tooltips;

namespace TT2026.Libraries.IzzysUI.Popups;

public struct PopupInfo
{
    private static string _textPopupSceneUID = "uid://csbee5jmj3v6x";
    private static PackedScene _textPopupScene;

    static PopupInfo()
    {
        _textPopupScene = ResourceLoader.Load<PackedScene>(_textPopupSceneUID);
    }
    
    public PopupType PopupType { get; set; }
    public string Header { get; set; }

    public PopupController InstantiateController(Control parent, TaskCompletionSource<object> tcs)
    {
        PopupController popupController;
        switch (PopupType)
        {
            case PopupType.Text: popupController = _textPopupScene.Instantiate<PopupController>(); break;
            default: throw new NotImplementedException();
        }
        popupController.Setup(parent, tcs);
        return popupController;
    }
}

public enum PopupType
{
    Text,
}