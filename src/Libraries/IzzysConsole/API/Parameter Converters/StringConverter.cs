namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
{
    [ParameterConverter(typeof(string))]
    public class StringConverter : IParameterConverter
    {
        public object Convert(string userValue) => userValue;
    }
}