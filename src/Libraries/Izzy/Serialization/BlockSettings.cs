using System.Linq;

namespace TT2026.libraries.Izzy.Serialization
{
    public class BlockSettings
    {
        public char BlockOpener;
        public char BlockCloser;
        public char ArrayOpener;
        public char ArrayCloser;
        public char ArraySeperator;
        public char ItemSeperator;
        public char SetOperator;
        public char BlockHeaderFinalizer;
        public char StringWrapper;
        public char MetadataSeperator;
        public char EscapeChar;
        public char[] customSpecialCharacters;

        public bool IsSpecialCharacter(char character) =>
            character == BlockOpener ||
            character == BlockCloser ||
            character == ArrayOpener ||
            character == ArrayCloser ||
            character == ArraySeperator ||
            character == ItemSeperator ||
            character == SetOperator ||
            character == BlockHeaderFinalizer ||
            character == StringWrapper ||
            customSpecialCharacters.Contains(character);
            
        public string AddEscapesForSpecialCharacters(string str)
        {
            string builtString = string.Empty;
            for (int i = 0; i < str.Length; i++)
            {
                char character = str[i];
                if (IsSpecialCharacter(character))
                    builtString += EscapeChar.ToString() + character.ToString();
                else
                    builtString += character;
            }

            return builtString;
        }

        public string WrapInString(string str) => StringWrapper + str + StringWrapper;
        
        public BlockSettings(char setOperator = '=', char opener = '{', char closer = '}', char arrayOpener = '[', char arrayCloser = ']', char arraySeperator = ',', char itemSeperator = ';', char blockHeaderFinalizer = ':', char stringWrapper = '"', char metadataSeperator = '|', char escapeChar = '\\', params char[] customSpecialCharacters)
        {
            this.SetOperator = setOperator;
            this.BlockOpener = opener;
            this.BlockCloser = closer;
            this.ArrayOpener = arrayOpener;
            this.ArrayCloser = arrayCloser;
            this.ArraySeperator = arraySeperator;
            this.ItemSeperator = itemSeperator;
            this.BlockHeaderFinalizer = blockHeaderFinalizer;
            this.StringWrapper = stringWrapper;
            this.MetadataSeperator = metadataSeperator;
            this.EscapeChar = escapeChar;
            this.customSpecialCharacters = customSpecialCharacters;
        }
    }
}