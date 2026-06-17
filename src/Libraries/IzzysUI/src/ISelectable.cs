namespace TT2026.libraries.IzzysUI;

public interface ISelectable
{
    public string Name { get; }
    public void OnSelected(bool debug);
}