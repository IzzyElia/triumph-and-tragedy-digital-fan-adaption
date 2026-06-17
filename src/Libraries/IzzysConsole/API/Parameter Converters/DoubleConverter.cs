namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
{
    [ParameterConverter(typeof(double))]
    public class DoubleConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (double.TryParse(userValue, out double f)) return f;
            else return null;
        }
    }
}