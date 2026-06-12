namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(bool))]
    public class BoolConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            if (bool.TryParse(userValue, out bool f)) return f;
            else
            {
                string lowercaseValue = userValue.ToLowerInvariant();
                switch (lowercaseValue)
                {
                    case "yes": return true;
                    case "no": return false;
                    case "y": return true;
                    case "n": return false;
                    case "t": return true;
                    case "f": return false;
                    case "true": return true;
                    case "false": return false;
                    default:
                        return null;
                }
            }
        }
    }
}