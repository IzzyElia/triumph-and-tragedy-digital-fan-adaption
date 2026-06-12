namespace IzzysConsole
{
    /// <summary>
    /// Thrown if an attempt to execute a console command fails due to user error (such as an invalid parameter value)
    /// </summary>
    public class ConsoleCommandUserErrorException : System.Exception
    {
        ConsoleCommandUserErrorException() { }
        public ConsoleCommandUserErrorException(string message) : base(message)
        {
        }
    }
}