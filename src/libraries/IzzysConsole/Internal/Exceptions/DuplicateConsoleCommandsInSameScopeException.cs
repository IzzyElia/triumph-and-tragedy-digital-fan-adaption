
namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if two or more commands are defined with the same name in the same scope (name referring to the name the command is called by, as defined in the <see cref="ConsoleCommandAttribute"/> attribute). 
    /// </summary>
    public class DuplicateConsoleCommandsInSameScopeException : System.Exception
    {
        public DuplicateConsoleCommandsInSameScopeException(ICommand a, ICommand b) : base($"Multiple console commands use the name '{a.commandName}' in the same scope\n'{a.commandName}' at {a.implimentingMember.DeclaringType.FullName}.{a.implimentingMember.Name}\nand '{b.commandName}' at {b.implimentingMember.DeclaringType.FullName}.{b.implimentingMember.Name}")
        {
        }
    }
}