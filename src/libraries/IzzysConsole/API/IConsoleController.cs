using IzzysConsole.Internal;

namespace IzzysConsole
{
    /// <summary>
    /// This interface is used to connect to the <see cref="ConsoleManager"/> and receive notifications when the console log changes
    /// </summary>
    public interface IConsoleController
    {
        public abstract bool IsActive { get; }
        public abstract bool IsFocused { get; }
        /// <summary>
        /// Called when the console log changes. Requires that the object be registered through <see cref="Register"/> or <see cref="ConsoleManager.RegisterConsole(IConsoleController)"/>
        /// </summary>
        /// <param name="text">The current FULL text of the console log</param>
        public abstract void OnConsoleLogChanged(string text);
        /// <summary>
        /// Returns the current full text of the console log
        /// </summary>
        public sealed string GetLog() => ConsoleManager.GetLog();
        /// <summary>
        /// Sends a command to the console for execution
        /// </summary>
        public sealed void SendCommand(string command) => ConsoleManager.TryExecuteCommand(command);
        /// <summary>
        /// Registers this console controller to be notified (through <see cref="OnConsoleLogChanged(string)"/>) when the console log changes
        /// </summary>
        /// <seealso cref="Unregister"/>
        public sealed void Register() => ConsoleManager.RegisterConsole(this);
        /// <summary>
        /// Unregisters the console, unsubscribing it from being notified when the console log changes
        /// </summary>
        /// <seealso cref="Register"/>
        public sealed void Unregister() => ConsoleManager.UnregisterConsole(this);
    }
}
