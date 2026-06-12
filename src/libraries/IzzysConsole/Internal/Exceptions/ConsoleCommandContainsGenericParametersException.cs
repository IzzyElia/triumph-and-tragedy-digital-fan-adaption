namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a command's method contains any generic parameters (this is unsupported)
    /// </summary>
    public class ConsoleCommandContainsGenericParametersException : ConsoleCommandDefinitionException
    {
        public ConsoleCommandContainsGenericParametersException() : base ("Console command methods cannot contain generic parameters") { }
    }
}