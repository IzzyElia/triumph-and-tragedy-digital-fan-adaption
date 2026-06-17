using TT2026.libraries.IzzysConsole.API;

namespace TT2026.libraries.IzzysConsole.Internal.Exceptions
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