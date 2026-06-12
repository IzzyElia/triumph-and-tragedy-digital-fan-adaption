namespace IzzysConsole.Internal
{
    /// <summary>
    /// Thrown if multiple <see cref="IParameterConverter"/>'s are defined for the same type
    /// </summary>
    public sealed class ParameterConverterCompetitionException : ParameterConverterDefinitionException
    {
        public ParameterConverterCompetitionException(System.Type competingParameterConverterTypeA, System.Type competingParameterConverterTypeB, System.Type competingType) : base($"Multiple {typeof(IParameterConverter).Name}'s are flagged for converting the type {competingType.Name} ({competingParameterConverterTypeA.GetType().FullName} and {competingParameterConverterTypeB.GetType().FullName})") { }
    }
}