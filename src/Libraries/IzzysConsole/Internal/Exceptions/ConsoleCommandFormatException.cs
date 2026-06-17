using TT2026.libraries.IzzysConsole.API;

namespace TT2026.libraries.IzzysConsole.Internal.Exceptions
{
    /// <summary>
    /// Thrown if the 'format' parameter of the <see cref="ConsoleCommandAttribute"/> attribute is improperly formatted.
    /// The expected format is "commandname [parameter 1] [parameter 2]" (etc...)
    /// </summary>
    public class ConsoleCommandFormatException : ConsoleCommandDefinitionException
    {
        public ConsoleCommandFormatException(string message) : base(message)
        {
        }
    }
}