namespace IzzysConsole.Internal
{
    /// <summary>
    /// Base class for any issue in the definition of a <see cref="IParameterConverter"/>
    /// </summary>
    /// <remarks></remarks>
    public abstract class ParameterConverterDefinitionException : System.Exception
    {
        ParameterConverterDefinitionException() { }
        protected ParameterConverterDefinitionException(string message) : base(message) { }
    }
}