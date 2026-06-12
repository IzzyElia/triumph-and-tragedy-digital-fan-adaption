namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if a <see cref="IParameterConverter"/> returned a type other than the type specified in its <see cref="ParameterConverterAttribute"/> attribute
    /// </summary>
    public sealed class ParameterConverterReturnedWrongTypeException : ParameterConverterDefinitionException
    {
        public ParameterConverterReturnedWrongTypeException(IParameterConverter converter, System.Type expectedType, System.Type returnedType) : base($"The parameter converter '{converter.GetType().Name}' returned an unexpected type (expected '{expectedType.Name}' but returned '{returnedType.Name}')") { }
    }
}