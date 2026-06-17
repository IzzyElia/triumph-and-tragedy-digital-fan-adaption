using System;
using Godot;

namespace TT2026.libraries.IzzysConsole.API.Parameter_Converters
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