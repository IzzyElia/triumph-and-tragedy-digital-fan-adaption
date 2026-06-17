using System.Threading.Tasks;
using Godot;

namespace TT2026.libraries.IzzysUI.Tooltips;

public abstract partial class PopupController : Control
{
    [Export] Button _cancelButton;
    [Export] Button _okButton;
    [Export] Label _popupHeader;
    private TaskCompletionSource<object> _tcs;

    public override void _Ready()
    {
        base._Ready();
        _okButton.Pressed += Submit;
        _cancelButton.Pressed += Close;
    }

    public void Setup(Control parent, TaskCompletionSource<object> tcs)
    {
        _tcs = tcs;
        parent.AddChild(this);
        SetPosition(Vector2.Zero);
        Vector2 prefabSize = GetSize();
        OffsetLeft = -prefabSize.X / 2;
        OffsetTop = -prefabSize.Y / 2;
        OffsetRight = prefabSize.X / 2;
        OffsetBottom = prefabSize.Y / 2;
    }

    private void Submit()
    {
        lock (_tcs)
        {
            if (_tcs.Task.IsCompleted) return;
            _tcs.SetResult(ReturnResult());
        }
        QueueFree();
    }

    protected abstract object ReturnResult();

    public void Refresh()
    {
        
    }

    public void Close()
    {
        lock (_tcs)
        {
            if (_tcs.Task.IsCompleted) return;
            _tcs.SetCanceled();
        }
        QueueFree();
    }
}