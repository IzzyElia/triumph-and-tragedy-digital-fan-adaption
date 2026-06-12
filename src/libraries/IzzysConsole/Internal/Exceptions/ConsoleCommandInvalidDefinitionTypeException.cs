namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if the <see cref="ConsoleCommandAttribute"/> is applied to a member that is not a method or property
    /// </summary>
    public class ConsoleCommandInvalidDefinitionTypeException : ConsoleCommandDefinitionException
    {
        public ConsoleCommandInvalidDefinitionTypeException (string message) : base (message) { }
    }
}