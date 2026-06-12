namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(sbyte))]
    public class SByteConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (sbyte.TryParse(userValue, out sbyte f)) return f;
            else return null;
        }
    }
}