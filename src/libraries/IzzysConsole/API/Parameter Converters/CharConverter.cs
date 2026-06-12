namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(char))]
    public class CharConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue.Length == 1) return userValue[0];
            else return null;
        }
    }
}