namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(float))]
    public class FloatConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (float.TryParse(userValue, out float f)) return f;
            else return null;
        }
    }
}