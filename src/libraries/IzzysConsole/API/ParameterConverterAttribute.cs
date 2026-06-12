namespace IzzysConsole
{
    /// <summary>
    /// Flags a class as the deticated parameter converter for a specified type. The associated class must also inherit from <see cref="IParameterConverter"/>
    /// </summary>
    [System.AttributeUsage(validOn: System.AttributeTargets.Class, Inherited = false)]
    public class ParameterConverterAttribute : System.Attribute
    {
        public System.Type associatedType { get; private set; }
        /// <param name="parameterType">The parameter type that this <see cref="IParameterConverter"/> is for</param>
        public ParameterConverterAttribute(System.Type parameterType)
        {
            this.associatedType = parameterType;
        }
    }
}