namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(ushort))]
    public class UShortConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (ushort.TryParse(userValue, out ushort f)) return f;
            else return null;
        }
    }
}