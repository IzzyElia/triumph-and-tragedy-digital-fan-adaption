namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a global console command is defined in a static class. This is unsupported, as the method cannot be called without the type's generic parameters also being specified.
    /// </summary>
    public class GlobalConsoleCommandInGenericClassException : System.Exception
    {
        public GlobalConsoleCommandInGenericClassException() : base($"Static method console commands in generic classes are not supported. Is there a way to define the command in a non-generic class?")
        {
        }
    }
}