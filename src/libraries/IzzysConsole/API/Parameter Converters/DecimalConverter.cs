namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(decimal))]
    public class DecimalConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (decimal.TryParse(userValue, out decimal f)) return f;
            else return null;
        }
    }
}