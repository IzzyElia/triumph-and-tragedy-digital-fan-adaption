namespace IzzysConsole.Internal.ParameterConverters
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