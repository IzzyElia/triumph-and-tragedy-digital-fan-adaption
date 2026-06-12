namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(int))]
    public class IntConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (int.TryParse(userValue, out int f)) return f;
            else return null;
        }
    }
}