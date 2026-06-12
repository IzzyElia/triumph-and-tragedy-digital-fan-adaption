namespace IzzysConsole
{
    /// <summary>
    /// Thrown if an attempt to execute a console command fails due to user error (such as an invalid parameter value)
    /// </summary>
    public class ConsoleCommandInsufficientParameterException : ConsoleCommandUserErrorException
    {
        public ConsoleCommandInsufficientParameterException(string commandName, int requiredParametersCount, string[] allParameterNames) : base($"Command '{commandName}' requires at least {requiredParametersCount} parameter{(requiredParametersCount == 1 ? string.Empty : 's')}: {ConvertParametersArrayToString(allParameterNames, requiredParametersCount)}")
        {
        }
        static string ConvertParametersArrayToString (string[] parametersArray, int numberToUse)
        {
            string output = "";
            int parametersToShow = System.Math.Min(numberToUse, parametersArray.Length);
            for (int i = 0; i < parametersToShow; i++)
            {
                output += $"[{parametersArray[i]}]";
                if (i < parametersToShow - 2)
                    output += ' ';
                else if (i == parametersToShow - 2)
                    output += " and ";
            }
            return output;
        }
    }
}