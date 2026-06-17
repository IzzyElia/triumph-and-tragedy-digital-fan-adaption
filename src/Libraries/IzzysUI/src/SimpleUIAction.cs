using System;
using System.Threading.Tasks;

namespace TT2026.libraries.IzzysUI;

// ReSharper disable once InconsistentNaming
public class SimpleUIAction(string name, Action action) : IAction
{
    public string Label => name;
    public void ExecuteAction()
    {
        action.Invoke();
    }

    public bool IsActionDisplayed() => true;

    public bool IsActionAllowed() => true;
}

// ReSharper disable once InconsistentNaming
public class SimpleUIActionAsync(string name, Func<Task<Exception>> action) : IAction
{
    public string Label => name;
    public void ExecuteAction()
    {
        action.Invoke();
    }

    public bool IsActionDisplayed() => true;

    public bool IsActionAllowed() => true;
}