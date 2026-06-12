using System;
using TT2026.IzzysUI.Tooltips;

namespace TT2026.IzzysUI;

// ReSharper disable once InconsistentNaming
public class SimpleUIAction : IAction
{
    private Action _action;
    private string _name;

    public SimpleUIAction(string name, Action action)
    {
        _name = name;
        _action = action;
    }

    public string Label => _name;
    public void ExecuteAction()
    {
        _action.Invoke();
    }

    public bool IsActionDisplayed() => true;

    public bool IsActionAllowed() => true;
}