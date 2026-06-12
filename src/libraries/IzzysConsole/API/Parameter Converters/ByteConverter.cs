namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(byte))]
    public class ByteConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (byte.TryParse(userValue, out byte f)) return f;
            else return null;
        }
    }
}