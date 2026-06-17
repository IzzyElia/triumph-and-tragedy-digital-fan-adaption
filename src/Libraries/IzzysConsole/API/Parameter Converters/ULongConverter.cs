namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
{
    [ParameterConverter(typeof(ulong))]
    public class ULongConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (ulong.TryParse(userValue, out ulong f)) return f;
            else return null;
        }
    }
}