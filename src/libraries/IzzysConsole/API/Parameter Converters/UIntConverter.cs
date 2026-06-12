namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(uint))]
    public class UIntConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (uint.TryParse(userValue, out uint f)) return f;
            else return null;
        }
    }
}