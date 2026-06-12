namespace IzzysConsole.Internal.ParameterConverters
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