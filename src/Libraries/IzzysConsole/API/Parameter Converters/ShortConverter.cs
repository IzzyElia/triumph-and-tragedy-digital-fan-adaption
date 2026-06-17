namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
{
    [ParameterConverter(typeof(short))]
    public class ShortConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (short.TryParse(userValue, out short f)) return f;
            else return null;
        }
    }
}