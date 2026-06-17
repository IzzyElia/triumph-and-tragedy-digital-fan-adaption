namespace TT2026.libraries.Izzy;

public interface INamedVariableHolder<T>
{
    public T GetNamedVariable(string name);
}