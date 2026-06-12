namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a console command is improperly defined
    /// </summary>
    public abstract class ConsoleCommandDefinitionException : System.Exception
    {

        protected ConsoleCommandDefinitionException(string message) : base(message)
        {
        }
    }
}