namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a <see cref="IParameterConverter"/> does not contain a public parameterless constructor
    /// </summary>
    public sealed class ParameterConverterDoesNotContainParameterlessConstructorException : ParameterConverterDefinitionException
    {
        public ParameterConverterDoesNotContainParameterlessConstructorException (System.Type parameterConverterType) : base ($"The parameter converter {parameterConverterType.Name} does not contain a public parameterless constructor") { }
    }
}