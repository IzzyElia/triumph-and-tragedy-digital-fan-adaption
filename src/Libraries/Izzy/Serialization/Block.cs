using System;
using System.Collections.Generic;
using TT2026.libraries.Izzy.UnitTesting;

namespace TT2026.libraries.Izzy.Serialization;

public class Block
{
    public string Header;

    private readonly List<BlockEntry> _items = new();
    public IReadOnlyList<BlockEntry> UnkeyedEntries => _items;

    private readonly Dictionary<string, BlockEntry> _keyedItems = new ();
    public IReadOnlyDictionary<string, BlockEntry> KeyedEntries => _keyedItems;

    private readonly Dictionary<string, BlockArray> _arrays = new ();
    public IReadOnlyDictionary<string, BlockArray> Arrays => _arrays;

    private readonly Dictionary<string, Block> _subBlocks = new();
    public IReadOnlyDictionary<string, Block> Children => _subBlocks;

    public readonly BlockSettings Settings;

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
            str += blockSettings.BlockOpener + "\n";
        }
        else
        {
            str += blockSettings.BlockOpener + blockSettings.WrapInString(Header) + blockSettings.BlockHeaderFinalizer + "\n";
        }
            
        foreach (BlockEntry item in _items)
        {
            if (item.Metadata == null)
                str += indent + "\t" + blockSettings.WrapInString(item.Value) + blockSettings.ItemSeperator + "\n";
            else
                str += indent + "\t" + item.Metadata + blockSettings.MetadataSeperator + blockSettings.WrapInString(item.Value) + blockSettings.ItemSeperator + "\n";

        }
        foreach ((string key, BlockEntry entry) in _keyedItems)
        {
            if (entry.Metadata == null)
                str += indent + "\t" + blockSettings.WrapInString(key) + blockSettings.SetOperator + blockSettings.WrapInString(entry.Value) + blockSettings.ItemSeperator + "\n";
            else
                str += indent + "\t" + blockSettings.WrapInString(key) + blockSettings.SetOperator + blockSettings.WrapInString(entry.Metadata) + blockSettings.MetadataSeperator + Settings.WrapInString(entry.Value) + blockSettings.ItemSeperator + "\n";

        }
        foreach (KeyValuePair<string, BlockArray> array in _arrays)
        {
            str += indent + "\t" + blockSettings.WrapInString(array.Key) + blockSettings.SetOperator + array.Value.BuildStringRecursively(indentDepth + 1, blockSettings) + "\n";
        }

        foreach (KeyValuePair<string, Block> subBlock in _subBlocks)
        {
            str += indent + "\t" + blockSettings.WrapInString(subBlock.Key) + blockSettings.SetOperator + subBlock.Value.BuildStringRecursively(indentDepth + 1, blockSettings) + "\n";
        }
        str += indent + blockSettings.BlockCloser;
        return str;
    }

    public void AddKeyedEntry(string key, string value, string metadata)
    {
        _keyedItems.Add(key, new BlockEntry(value, metadata));
    }

    public void AddEntry(string key, string value, string metadata)
    {
        if (key != null)
        {
            AddKeyedEntry(key, value, metadata);
        }
        else
        {
            AddUnkeyedEntry(value, metadata);
        }
    }

    public void AddArray(string key, BlockArray array)
    {
        _arrays.Add(key, array);
    }

    public void AddUnkeyedEntry(string entry, string metadata)
    {
        _items.Add(new BlockEntry(entry, metadata));
    }

    public void AddChild(string key, Block child)
    {
        _subBlocks.Add(key, child);
    }

    public Block GetChild(string key)
    {
        if (_subBlocks.TryGetValue(key, out Block childBlock))
        {
            return childBlock;
        }
        else
        {
            return null;
        }
    }

    public Block(BlockSettings blockSettings)
    {
        this.Settings = blockSettings;
    }

    public Block(string str, BlockSettings blockSettings) : this(blockSettings)
    {
        char setOperator = blockSettings.SetOperator;
        char itemSeperator = blockSettings.ItemSeperator;
        char blockHeaderFinalizer = blockSettings.BlockHeaderFinalizer;


        string currentString = string.Empty;
        bool insideDefinition = false;
        bool settingItem = false;
        bool settingHeader = true;
        string currentKey = string.Empty;
        bool insideString = false;
        bool buildingSubelement = false;
        int subelementDepth = 0;
        string subelementString = string.Empty;
        char subelementOpener = char.MinValue;
        char subelementCloser = char.MinValue;
        bool subelementIsArray = false;
        string subelementKey = string.Empty;
        string debuggingString = string.Empty;
        string metadata = string.Empty;
            
        bool ignoringSpecialCharacter = false;
            
            
        for (int i = 0; i < str.Length; i++)
        {
            char character = str[i];
            debuggingString += character;
            if (debuggingString.Length > 50)
            {
                debuggingString = debuggingString.Substring(1, debuggingString.Length - 1);
            }
                
            // Ignore these characters completely (newline, tab, and space)
            if (character == '\n' || character == '\t' || character == ' ' && !insideString)
                continue;

            if (!insideDefinition)
            {
                if (character == blockSettings.BlockOpener)
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
                    continue;
                }
                    
                if (character == subelementOpener && !ignoringSpecialCharacter && !insideString)
                {
                    subelementDepth += 1;
                    subelementString += character;
                }
                else if (character == subelementCloser && !ignoringSpecialCharacter && !insideString)
                {
                    subelementDepth -= 1;
                    subelementString += character;
                    if (subelementDepth == 0)
                    {
                        buildingSubelement = false;
                        if (subelementIsArray)
                        {
                            _arrays.Add(subelementKey, new BlockArray(subelementString, blockSettings));
                        }
                        else
                        {
                            _subBlocks.Add(subelementKey, new Block(subelementString, blockSettings));
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
                if (settingItem)
                {
                    subelementKey = currentKey;
                    currentKey = null;
                    settingItem = false;
                    settingHeader = false;
                }
                else
                {
                    throw new ArgumentException($"Unnamed subelement at '{debuggingString}'");
                }
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
                throw new ArgumentException($"Unexpected array closing symbol {blockSettings.ArrayCloser} while not inside an array definition at '{debuggingString}'");
            }
            else if (character == blockSettings.BlockOpener)
            {
                if (settingItem)
                {
                    subelementKey = currentKey;
                    currentKey = null;
                    settingItem = false;
                    settingHeader = false;
                }
                else
                {
                    throw new ArgumentException($"Unnamed subelement at '{debuggingString}'");
                }
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
                // If the code reaches this point, we know that we are in the definition for this block and therefore have encountered its closer
                break;
            }
            else if (character == blockHeaderFinalizer)
            {
                if (!settingHeader)
                    throw new ArgumentException($"Unexpected {blockHeaderFinalizer} symbol at '{debuggingString}'");

                Header = currentString;
                currentString = string.Empty;
                settingHeader = false;
            }
            else if (character == itemSeperator)
            {
                settingHeader = false;
                if (metadata.Length == 0)
                    metadata = null;
                if (settingItem)
                {
                    settingItem = false;
                    _keyedItems.Add(currentKey, new BlockEntry(currentString.Trim(), metadata));
                    currentKey = null;
                }
                else
                {
                    _items.Add(new BlockEntry(currentString.Trim(), metadata));
                }
                currentString = string.Empty;
                metadata = string.Empty;
            }
            else if (character == blockSettings.MetadataSeperator)
            {
                metadata = currentString;
                currentString = string.Empty;
            }
            else if (character == setOperator)
            {
                settingHeader = false;
                settingItem = true;
                currentKey = currentString.Trim();
                currentString = string.Empty;
            }
            else
            {
                currentString += character;
            }
        }
    }
}

public struct BlockEntry
{
    public string Metadata;
    public string Value;
         
    public BlockEntry(string value, string metadata)
    {
        Metadata = metadata;
        Value = value;
    }
}


public class BlockUnitTests
{
    [Test]
    TestResult TestAddingEscapeCharacters()
    {
        BlockSettings format = new BlockSettings(customSpecialCharacters: new char[] { '(', ')' });
        string input = "Test ( g } \\g \\\\g g";
        string expectedOutput = "Test \\( g \\} \\g \\\\g g";
        string output = format.AddEscapesForSpecialCharacters(input);
        if (output != expectedOutput)
            return new TestResult(false, $"Expected: {expectedOutput}\nGot     : {output}");
        else return new TestResult(true);
    }
}