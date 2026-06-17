namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
{
    [ParameterConverter(typeof(long))]
    public class LongConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (long.TryParse(userValue, out long f)) return f;
            else return null;
        }
    }
}