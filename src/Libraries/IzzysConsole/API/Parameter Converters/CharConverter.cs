namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
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