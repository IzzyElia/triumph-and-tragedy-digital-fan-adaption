namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a <see cref="IParameterConverter"/> is defined with generic parameters. This is unsupported
    /// </summary>
    public sealed class ParameterConverterContainsGenericParametersException : ParameterConverterDefinitionException
    {
        public ParameterConverterContainsGenericParametersException(System.Type converter) : base($"The parameter converter '{converter.GetType().Name}' is generic, which is unsupported") { }
    }
}