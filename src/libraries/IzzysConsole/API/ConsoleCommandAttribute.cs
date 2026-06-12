using System.Collections.Generic;
using IzzysConsole.Internal;

namespace IzzysConsole
{
    /// <summary>
    /// Flags a method as a console command. Static methods will be added to the global namespace, while instance methods can be accessed through scoping
    /// </summary>
    [System.AttributeUsage(validOn: System.AttributeTargets.Method | System.AttributeTargets.Property, AllowMultiple = false)]
    public class ConsoleCommandAttribute : System.Attribute
    {
        public string helpInfo { get; private set; }
        public string commandName { get; private set; }
        public string[] parameterKeys { get; private set; }
        public bool trailingFinalParameter { get; private set; }
        public bool scopingAllowed { get; private set; }
        /// <param name="name">The name the command is called by. The name cannot contain spaces, and any spaces will be removed</param>
        /// <param name="helpInfo">The string that will be shown when the user runs the 'help' command on this command</param>
        /// <param name="parameterNameOverrides">
        /// Overwrite the names of parameters in order (the first element of the array will become the name of the first parameter etc...). 
        /// Any unnamed parameters will be automatically named assuming camelCasing (etc myPamaterOne will become [My Parameter One]
        /// </param>
        /// <param name="allowUseInScoping">
        /// By default, commands CANNOT be used in scoping. Set this value to true if the command is intended and safe for use in scoping. For more information, refer to <see href="../articles/scoping.md">Command Scoping</see>
        /// </param>
        /// <param name="trailingFinalParameter">
        /// <para>
        /// If FALSE, any extra parameters entered after the final parameter will be ignored.
        /// For example, the parameters in the command 'myCommandWithTwoParameters somevalue somestring somestring extra_undefined_parameter' would be parsed as "somevalue" "somestring". Note how the third parameter, 'extra_undefined_parameter', is discarded.
        /// </para>
        /// <para>
        /// If TRUE, any extra parameters entered after the final parameter will be added to the final parameter string.
        /// For example, the parameters in the same command 'myCommandWithTwoParameters somevalue somestring extra_undefined_parameter' would be parsed as "somevalue" "somestring extra_undefined_parameter". Notice how the extra text is preserved and added to the final parameter
        /// </para>
        /// </param>
        public ConsoleCommandAttribute(string name, string helpInfo = null, string[] parameterNameOverrides = null, bool allowUseInScoping = false, bool trailingFinalParameter = true)
        {
            this.commandName = name;
            this.parameterKeys = parameterNameOverrides;

            if (helpInfo == null)
                this.helpInfo = "";
            else
                this.helpInfo = helpInfo;
            this.parameterKeys = parameterNameOverrides == null ? new string[0] : parameterNameOverrides;
            this.scopingAllowed = allowUseInScoping;
            this.trailingFinalParameter = trailingFinalParameter;
        }
    }
}