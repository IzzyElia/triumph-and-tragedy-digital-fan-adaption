namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a class is flagged with the <see cref="ParameterConverterAttribute"/>, but does not itself derive from <see cref="IParameterConverter"/>
    /// </summary>
    public sealed class ParameterConverterDoesNotDeriveException : ParameterConverterDefinitionException
    {
        public ParameterConverterDoesNotDeriveException(System.Type classFlaggedWithParameterConverterAttribute) : base ($"The class {classFlaggedWithParameterConverterAttribute} is flagged with the [ParameterConverter] attribute but does not derive from {typeof(IParameterConverter).Name}") { }
    }
}