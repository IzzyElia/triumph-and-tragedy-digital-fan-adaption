namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if the user enters an invalid parameter value when entering a console command
    /// </summary>
    /// <remarks>If an implimentation of <see cref="IParameterConverter.Convert(string)"/> returns a null value, it's interpreted as an invalid user input, and this error will be thrown.</remarks>
    public sealed class ConsoleCommandParameterParseException : ConsoleCommandUserErrorException
    {
        public ConsoleCommandParameterParseException(string keyOfFailedParameter, System.Type expectedParameterType) : base($"Could not parse parameter {keyOfFailedParameter} - {keyOfFailedParameter} must be a valid {expectedParameterType.GetType().Name}")
        {
        }
    }
}