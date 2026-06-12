using System;
using Godot;

namespace IzzysConsole.Internal.ParameterConverters
{
    [ParameterConverter(typeof(Color))]
    public class ColorConverter : IParameterConverter
    {
        public object Convert(string userValue)
        {
            if (userValue == null) return null;
            try
            {
                return Color.FromHtml(userValue);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}