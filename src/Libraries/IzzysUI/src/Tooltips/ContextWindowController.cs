using Godot;

namespace TT2026.libraries.IzzysUI.Tooltips;

public partial class ContextWindowController : DisplayController
{
    public static ContextWindowController ContextWindowUnderMouse { get; private set; }
    
	public override UIWindowCategory CategoryTag => UIWindowCategory.IndependantPopup;
	[Export] private Control MainContainer;
	[Export] private Label HeaderText;
	[Export] private Label MainText;
	[Export] private Control ActionsContainer;
	[Export] private Control FurtherReadingContainer;
	[Export] private Control StatContainer;
	
	private ContextWindowInfo _contextWindowInfo;
	private ContextWindowController _linkedWindow = null;
	private ContextWindowController _root;
	
	public static ContextWindowController Instantiate(ContextWindowInfo contextWindowInfo, Vector2 idealPosition, Control parentNode = null)
	{
		ContextWindowController contextWindow = Prefabs.ContextWindowPrefab.Instantiate<ContextWindowController>();
		parentNode.AddChild(contextWindow);
		contextWindow.SetTo(contextWindowInfo);
		contextWindow.GlobalPosition = idealPosition;
		contextWindow._root = contextWindow;
		
		return contextWindow;
	}

	protected override void Close()
	{
		_linkedWindow?.Close();
		
		QueueFree();
	}

	public override void Refresh()
	{
		_root.RefreshCascade();
	}

	private void RefreshCascade()
	{
		if (_contextWindowInfo.Source is not null)
			SetTo(_contextWindowInfo.Source.GetContextWindow());
		_linkedWindow?.RefreshCascade();
	}
	
	private void SetTo(ContextWindowInfo contextWindowInfo)
	{
		_contextWindowInfo = contextWindowInfo;
		if (contextWindowInfo.Text is null) MainText.Visible = false;
		else
		{
			MainText.Visible = true;
			MainText.Text = contextWindowInfo.Text;
		}
		if (contextWindowInfo.Header is null) HeaderText.Visible = false;
		else
		{
			HeaderText.Visible = true;
			HeaderText.Text = contextWindowInfo.Header;
		}
		
		foreach (var actionNode in ActionsContainer.GetChildren())
		{
			Button actionButton = actionNode.GetNode<Button>("Button");
			actionNode.QueueFree();
		}

		foreach (var linkNode in FurtherReadingContainer.GetChildren())
		{
			linkNode.QueueFree();
		}
		
		foreach (var statNode in StatContainer.GetChildren())
		{
			statNode.QueueFree();
		}

		foreach (var action in contextWindowInfo.Actions)
		{
			if (!action.IsActionDisplayed()) continue;
			UIActionController actionNode = Prefabs.TooltipActionPrefab.Instantiate<UIActionController>();
			ActionsContainer.AddChild(actionNode);
			actionNode.SetAction(action);
		}
		
		foreach (ITooltipLink link in contextWindowInfo.Links)
		{
			ContextWindowLinkController linkButton = Prefabs.TooltipLinkPrefab.Instantiate<ContextWindowLinkController>();
			FurtherReadingContainer.AddChild(linkButton);
			linkButton.Setup(this, link.Target);
		}
		
		foreach (var stat in contextWindowInfo.Stats)
		{
			UIStatDisplayController statDisplay = Prefabs.TooltipActionPrefab.Instantiate<UIStatDisplayController>();
			StatContainer.AddChild(statDisplay);
		}
	}
	
	public void SetOpenLink(ILinkable target)
	{
		if (_linkedWindow is not null) _linkedWindow.Close();
		ContextWindowInfo _linkedInfo = target.GetContextWindow();
		_linkedWindow = ContextWindowController.Instantiate(_linkedInfo, MainContainer.GlobalPosition + new Vector2(MainContainer.Size.X, 0), GetParent<Control>());
		_linkedWindow._root = _root;
	}
	
	// TODO this relies on execution order
	private bool _mouseOver = false;
	public void _MouseEnteredElement()
	{
		_mouseOver = true;
		ContextWindowUnderMouse = this;
	}

	public void _MouseExitedElement()
	{
		_mouseOver = false;
		ContextWindowUnderMouse = null;
	}
}