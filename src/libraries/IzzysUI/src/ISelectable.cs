namespace TT2026.IzzysUI;

public interface ISelectable
{
    public string Name { get; }
    public void OnSelected(bool debug);
}