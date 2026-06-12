namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if there is no <see cref="IParameterConverter"/> defined for the type of one or more of a commands parameters. 
    /// Built in support exists for all built in primitive types (int, string, etc...), and UnityEngine.Color. If you want to use other types
    /// as parameters, you can define conversions yourself by creating a class inheriting from <see cref="IParameterConverter"/> and adding the <see cref="ParameterConverterAttribute"/> attribute.
    /// </summary>
    public sealed class ConsoleCommandNoConverterForParameterException : ConsoleCommandDefinitionException
    {
        public ConsoleCommandNoConverterForParameterException(System.Type parameterType) : base($"No parameter converter is defined for the parameter type {parameterType.Name}\n\nTo add a custom parameter converter, create a class deriving from ParameterConverter, give it the [ParameterConverter(Type)] attribute, and impliment the Parameterconverter.Conver(string) method") { }
    }
}