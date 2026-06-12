namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(string))]
    public class StringConverter : IParameterConverter
    {
        public object Convert(string userValue) => userValue;
    }
}