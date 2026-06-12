using System;
using System.Collections.Generic;
using System.Linq;

namespace TT2026.IzzysUI.Tooltips;

/// <summary>
/// Contains all the info needed to construct a tooltip
/// </summary>
public class ContextWindowInfo
{
    public IContextWindowCreator Source { get; private set; }
    public string Text { get; private set; }
    public string Header { get; private set; }
    private IUIStat[] _stats;
    private ITooltipLink[] _links;
    private IAction[] _actions;
    public IReadOnlyList<ITooltipLink> Links => _links;
    public IReadOnlyList<IAction> Actions => _actions;
    public IReadOnlyList<IUIStat> Stats => _stats;

    public ContextWindowInfo(IContextWindowCreator source = null, string header = null, string text = null, IReadOnlyCollection<IUIInteraction> interactions = null)
    {
        this.Source = source;
        this.Text = text;
        this.Header = header;
        if (interactions is null)
        {
            _links = Array.Empty<ITooltipLink>();
            _actions = Array.Empty<IAction>();
            _stats = Array.Empty<IUIStat>();
        }
        else
        {
            _links = interactions.OfType<ITooltipLink>().ToArray();
            _actions = interactions.OfType<IAction>().ToArray();
            _stats = interactions.OfType<IUIStat>().ToArray();
        }
    }
}

