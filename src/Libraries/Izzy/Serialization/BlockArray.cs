using System;
using System.Collections.Generic;

namespace TT2026.libraries.Izzy.Serialization
{
    public class BlockArray
    {
        public readonly BlockSettings Settings;
        private List<BlockArrayEntry> _elements = new ();
        public string Header;
        public BlockArrayEntry[] Elements => _elements.ToArray();

        public void AddElement(object element, string metadata)
        {
            if (element is string str)
            {
                _elements.Add(new BlockArrayEntry(str, metadata));
            }
            else if (element is Block block)
            {
                _elements.Add(new BlockArrayEntry(block, metadata));
            }
            else if (element is BlockArray blockArray)
            {
                _elements.Add(new BlockArrayEntry(blockArray, metadata));
            }
            else
            {
                throw new ArgumentException("Invalid type (must be a string or a block)");
            }
        }

        public string BuildStringRecursively(int indentDepth, BlockSettings blockSettings)
        {
            string indent = string.Empty;
            for (int i = 0; i < indentDepth; i++)
            {
                indent += "\t";
            }
            string str = string.Empty;
            
            if (Header == null)
            {
                str += blockSettings.ArrayOpener.ToString() + "\n";
            }
            else
            {
                str += blockSettings.ArrayCloser.ToString() + blockSettings.WrapInString(Header) + blockSettings.BlockHeaderFinalizer + "\n";
            }
            for (int i = 0; i < _elements.Count; i++)
            {
                object element = _elements[i].Value;
                string metadata = _elements[i].Metadata;
                if (element is string elementStr)
                {
                    if (metadata == null)
                        str += indent + "\t" + blockSettings.WrapInString(elementStr);
                    else
                        str += indent + "\t" + blockSettings.WrapInString(metadata) + blockSettings.MetadataSeperator + blockSettings.WrapInString(elementStr);
                }
                else if (element is Block block)
                {
                    if (metadata == null)
                        str += indent + block.BuildStringRecursively(indentDepth + 1, blockSettings);
                    else
                        str += indent + blockSettings.WrapInString(metadata) + blockSettings.MetadataSeperator + 
                               block.BuildStringRecursively(indentDepth + 1, blockSettings);
                }
                else if (element is BlockArray array)
                {
                    if (metadata == null)
                        str += indent + array.BuildStringRecursively(indentDepth + 1, blockSettings);
                    else
                        str += indent + blockSettings.WrapInString(metadata) + blockSettings.MetadataSeperator +
                               array.BuildStringRecursively(indentDepth + 1, blockSettings);

                }

                if (i != _elements.Count - 1)
                {
                    str += blockSettings.ArraySeperator.ToString();
                }

                str += "\n";
            }
            str += indent + blockSettings.ArrayCloser;
            return str;
        }

        public BlockArray(BlockSettings blockSettings)
        {
            this.Settings = blockSettings;
        }

        public BlockArray(string str, BlockSettings blockSettings) : this(blockSettings)
        {
            string currentString = string.Empty;
            string metadata = string.Empty;
            bool insideDefinition = false;
            bool settingHeader = true;
            bool ignoringSpecialCharacter = false;
            bool insideString = false;
            bool buildingSubelement = false;
            int subelementDepth = 0;
            string subelementString = string.Empty;
            char subelementOpener = char.MinValue;
            char subelementCloser = char.MinValue;
            bool subelementIsArray = false;
            bool betweenSubelementAndSeperator = false;
            for (int i = 0; i < str.Length; i++)
            {
                char character = str[i];
                
                // Ignore these characters completely (newline, tab, and space)
                if (character == '\n' || character == '\t' || character == ' ' && !insideString)
                    continue;
                
                if (!insideDefinition)
                {
                    if (character == blockSettings.ArrayOpener)
                    {
                        insideDefinition = true;
                        continue;
                    }
                        
                    else 
                        continue;
                }
                
                if (buildingSubelement)
                {
                    if (ignoringSpecialCharacter)
                    {
                        subelementString += character;
                        ignoringSpecialCharacter = false;
                    }
                    
                    else if (character == subelementOpener && !ignoringSpecialCharacter && !insideString)
                    {
                        subelementDepth += 1;
                        subelementString += character;
                    }
                    else if (character == subelementCloser && !ignoringSpecialCharacter && !insideString)
                    {
                        subelementString += character;
                        subelementDepth -= 1;
                        if (subelementDepth == 0)
                        {
                            buildingSubelement = false;
                            betweenSubelementAndSeperator = true;
                            if (subelementIsArray)
                            {
                                _elements.Add(new BlockArrayEntry(new BlockArray(subelementString, blockSettings), metadata));
                            }
                            else
                            {
                                _elements.Add(new BlockArrayEntry(new Block(subelementString, blockSettings), metadata));
                            }

                            metadata = string.Empty;
                        }
                    }
                    else if (character == blockSettings.StringWrapper)
                    {
                        insideString = !insideString;
                        subelementString += character;
                    }
                    else if (character == blockSettings.EscapeChar)
                    {
                        ignoringSpecialCharacter = true;
                        subelementString += character;
                    }
                    else
                    {
                        subelementString += character;
                    }
                    continue;
                }

                if (ignoringSpecialCharacter)
                {
                    currentString += character;
                    ignoringSpecialCharacter = false;
                    continue;
                }
                
                if (character == blockSettings.EscapeChar)
                {
                    ignoringSpecialCharacter = true;
                    continue;
                }
                else if (insideString && character != blockSettings.StringWrapper)
                {
                    currentString += character;
                    continue;
                }
                else if (character == blockSettings.StringWrapper)
                {
                    if (insideString)
                    {
                        insideString = false;
                    }
                    else
                    {
                        insideString = true;
                    }
                }
                else if (character == blockSettings.ArrayOpener)
                {
                    if (currentString.Length > 0)
                        throw new ArgumentException("Characters before start of subarray definition in array");
                    subelementDepth = 1;
                    buildingSubelement = true;
                    subelementString = string.Empty;
                    subelementString += blockSettings.ArrayOpener;
                    subelementOpener = blockSettings.ArrayOpener;
                    subelementCloser = blockSettings.ArrayCloser;
                    subelementIsArray = true;
                }
                else if (character == blockSettings.ArrayCloser)
                {
                    // If defining an array, all logic is handled above.
                    // If the code reaches this point, we know that we are not defining an array.
                    if (currentString.Length > 0)
                    {
                        if (metadata.Length == 0) 
                            metadata = null;
                        _elements.Add(new BlockArrayEntry(currentString, metadata));
                    }
                    break;
                }
                else if (character == blockSettings.BlockOpener)
                {
                    if (currentString.Length > 0)
                        throw new ArgumentException("Characters before start of subblock definition in array");
                    currentString = string.Empty;
                    buildingSubelement = true;
                    subelementString = string.Empty;
                    subelementString += blockSettings.BlockOpener;
                    subelementOpener = blockSettings.BlockOpener;
                    subelementCloser = blockSettings.BlockCloser;
                    subelementDepth = 1;
                    subelementIsArray = false;
                }
                else if (character == blockSettings.BlockCloser)
                {
                    // If defining a block , all logic is handled above.
                    // If the code reaches this point, we know that we are not defining a block.
                    throw new ArgumentException($"Unexpected block closing symbol {blockSettings.ArrayCloser} while not inside a block definition");
                }
                else if (character == blockSettings.BlockHeaderFinalizer)
                {
                    if (!settingHeader)
                        throw new ArgumentException($"Unexpected '{blockSettings.BlockHeaderFinalizer}' symbol in array definition {str}");

                    Header = currentString;
                    currentString = string.Empty;
                    settingHeader = false;
                }
                else if (character == blockSettings.MetadataSeperator)
                {
                    metadata = currentString;
                    currentString = string.Empty;
                }
                else if (character == blockSettings.ArraySeperator)
                {
                    settingHeader = false;
                    betweenSubelementAndSeperator = false;
                    
                    if (currentString.Length > 0)
                    {
                        if (metadata.Length == 0) 
                            metadata = null;
                        _elements.Add(new BlockArrayEntry(currentString, metadata));
                        metadata = string.Empty;
                        currentString = string.Empty;
                    }
                }
                else
                {
                    currentString += character;
                }
            }
        }
    }

    public class BlockArrayEntry
    {
        public string Metadata;
        public object Value;
        public BlockArrayEntry(object value, string metadata)
        {
            this.Metadata = metadata;
            this.Value = value;
        }
    }
}