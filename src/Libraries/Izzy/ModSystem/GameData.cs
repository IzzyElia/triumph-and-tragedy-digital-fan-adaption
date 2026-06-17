using System;
using System.Collections.Generic;
using System.IO;
using TT2026.libraries.Izzy.ForcedInitialization;
using TT2026.libraries.Izzy.Utils;

namespace TT2026.libraries.Izzy.ModSystem
{
    public class GameDataSystem
    {
        public GameDataSystem() { }
    }
    [Serializable]
    [ForceInitialize]
    public class GameData : DataNode
    {
        const string GameDataDirectory = "defines";
        public const string rootStr = "definition_root";
        static GameData ()
		{
            ModHandler.OnUnloadingMods += Reset;
            ModHandler.OnLoadMod += Load;
            ModHandler.OnModStateChanged += Lock;
		}
        static void Reset ()
		{
            new GameData().RegisterAs(GameData.rootStr);
        }
        static void Load (DirectoryInfo directory)
        {
            string modPath = Path.Combine(directory.FullName, GameDataDirectory);
            DirectoryInfo directoryInfo = new DirectoryInfo(modPath);
            if (directoryInfo.Exists)
                GameData.LoadFromFolderRecursively(directoryInfo);
            else
            {
                DynamicLogger.LogWarning($"Mod {directory.Name} has no defines folder");
            }
        }
        static void Lock()
        {
            foreach (Reference reference in referenceQueue)
			{
                Utilities.AssignFromReference(reference);
			}
        }
        struct Reference
        {
            public Property property; public string path;
            public Reference(Property property, string reference) { this.property = property; this.path = reference; }
        }
        static HashSet<Reference> referenceQueue = new HashSet<Reference>();
        static GameData root
		{
            get
			{
                return (GameData)NamedNode(rootStr);
			}
		}
        static void AddReference(Property property, string reference)
		{
            referenceQueue.Add(new Reference(property, reference));
		}
        public static void LoadFromFolderRecursively(DirectoryInfo directory)
        {
            if (!directory.Exists) { throw new System.ArgumentException($"directory {directory.FullName} does not exist"); }
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo subDirectory in subDirectories)
            {
                LoadFromFolderRecursively(subDirectory);
            }
            FileInfo[] FilesInFolder = directory.GetFiles();
            foreach (FileInfo CurrentFile in FilesInFolder)
            {
                if (Path.GetExtension(CurrentFile.FullName) == ".txt")
                {
                    LoadDataFile(CurrentFile);
                }
            }
        }
        static void LoadDataFile(FileInfo file)
        {
            StreamReader stream = file.OpenText();
            List<string> lines = new List<string>();
            while (!stream.EndOfStream)
            {
                lines.Add(stream.ReadLine());
            }
            stream.Close();
            FillOutFromPreparedScript(Utilities.PrepareParsableCodeString(lines));
        }
        static void FillOutFromPreparedScript(string code)
        {
            GameData scope = (GameData)NamedNode(rootStr); //always start at the root
            string contentInCurrentContext = "";
            for (int i = 0; i < code.Length; i++)
            {
                char character = code[i];
                switch (character)
                {
                    case Utilities.blockOpen:
                        scope = Utilities.SetScopeFromHeaderString(contentInCurrentContext, scope);
                        contentInCurrentContext = "";
                        break;
                    case Utilities.blockClose:
                        scope = (GameData)scope.Parent;
                        contentInCurrentContext = "";
                        break;
                    case Utilities.lineBreak:
                        scope.AddProperty(Utilities.CreatePropertyFromString(contentInCurrentContext));
                        contentInCurrentContext = "";
                        break;
                    default:
                        contentInCurrentContext += character;
                        break;
                }
            }
        }
        public static GameData[] All
        {
            get
            {
                DataNode[] all = root.Children;
                GameData[] data = new GameData[all.Length];
				for (int i = 0; i < all.Length; i++)
				{
                    data[i] = (GameData)all[i];
				}
                return data;
            }
        }
        public static GameData Get (string key)
		{
            GameData data = (GameData)root.GetChild(new NameTypePair(key));
            if (data == null)
                throw new InvalidOperationException($"Invalid game data reference '{key}'");
            return data;
        }
        public static GameData[] GetAllOfType(string type)
		{
            List<DataNode> children = root.GetChildrenOfType(type);
            GameData[] data = new GameData[children.Count];
			for (int i = 0; i < data.Length; i++)
			{
                data[i] = (GameData)children[i];
			}
            return data;
        }
        public static void PrintGameDataTree ()
        {
            PrintGameDataTreeRecursively(root, 0);
        }
        static void PrintGameDataTreeRecursively (GameData data, int tabs)
        {
            string str = "";
            for (int i = 0; i < tabs; i++)
                str += '\t';

            foreach(GameData child in data.children)
            {
                DynamicLogger.Log(str + child.NameAndType.ToString());
                PrintGameDataTreeRecursively(child, tabs + 1);
            }
        }
        class Utilities
        {
            public const char lineBreak = ';';
            public const char blockOpen = '{';
            public const char blockClose = '}';
            public const char paramsOpen = '(';
            public const char paramsClose = ')';
            public const char setOperator = '=';
            public const char typeNameSeperator = ':';
            public const char paramSeperator = ',';
            public const char valueSeperator = ',';
            public const char stackSeperator = '.';
            public const char getValueMarker = '@';
            public const string defaultParameterType = "string";
            public const char commentLineMarker = '#';
            static HashSet<char> ignoreLineBreakCharacters = new HashSet<char>
            {
                lineBreak,
                blockOpen,
                blockClose,
                setOperator
            };
            public static string PrepareParsableCodeString(List<string> lines)
            {
                string code = "";
                for (int i = 0; i < lines.Count; i++)
                {
                    string line = lines[i];
                    //remove anything after a comment marker (if found)
                    if (line.Contains(commentLineMarker.ToString()))
                    {
                        line = line.Split(commentLineMarker)[0];
                    }
                    //if line is just whitespace after removing the comment go to next line
                    if (line.Trim() == "") { continue; }
                    //if the last character of the line is just generic text, a parameter closer, etc. add a semicolon
                    char lastCharacter = StringUtils.GetLastCharacter(line);
                    if (!ignoreLineBreakCharacters.Contains(lastCharacter))
                    {
                        line += lineBreak;
                    }
                    //Add the formatted line to the prepped script
                    code += line;
                }
                code = StringUtils.RemoveWhitespace(code); //cleanup whitespace
                return code;
            }
            public static Property[] ParseParameterStrings(string rawParameters)
            {
                //Extract each entry (within, the, brakets) as a seperate string
                List<Property> parameters = new List<Property>();
                if (rawParameters.Length > 0)
                {
                    string[] parameterStrings = rawParameters.Split(paramSeperator);
                    for (int i = 0; i < parameterStrings.Length; i++)
                    {
                        Property newParameter = CreatePropertyFromString(parameterStrings[i], "parameter");
                        parameters.Add(newParameter);
                    }
                }


                return parameters.ToArray();
            }
            public static GameData SetScopeFromHeaderString(string definition, GameData scope)
            {
                string stage = "type";
                string type = "";
                string name = "";
                string paramsString = "";
                for (int i = 0; i < definition.Length; i++)
                {
                    char character = definition[i];
                    if (character == setOperator) { break; }
                    else if (character == typeNameSeperator) { stage = "name"; continue; }
                    else if (character == paramsOpen) { stage = "parameters"; continue; }
                    else if (character == paramsClose) { stage = "finalize"; continue; }
                    else
                    {
                        switch (stage)
                        {
                            case "type":
                                type += character;
                                break;
                            case "name":
                                name += character;
                                break;
                            case "parameters":
                                paramsString += character;
                                break;
                            default:
                                throw new System.ArgumentException($"Error parsing {definition}");
                                //break;
                        }
                    }
                }
                if (name == "") { name = type; }
                GameData prexisting = (GameData)scope.GetChild(name, type);
                GameData dataItem;
                int preexistingCount = 1;
                int failsafe = 999;
                string nameMod = "";
                while (prexisting != null)
                {
                    preexistingCount++;
                    if (preexistingCount > failsafe)
                    {
                        DynamicLogger.LogWarning($"Too many items by the name of {name} in {scope}");
                        dataItem = prexisting;
                        break;
                    }
                    nameMod = "_" + preexistingCount;
                    prexisting = (GameData)scope.GetChild(name + nameMod, type);
                }
                dataItem = new GameData();
                dataItem.Type = type;
                dataItem.Name = name + nameMod;
                dataItem.Parent = scope;
                dataItem.AddProperties(ParseParameterStrings(paramsString));
                return dataItem;
            }
            /// <summary>
            /// Creates a property from a string such that myType:myName = myValue becomes
            /// type == myType
            /// name == myName
            /// value == myValue
            /// </summary>
            /// <param name="definition">The string used to create the property, formated as type(optional):name = value</param>
            /// <param name="forceType">Overwrites the property type</param>
            /// <returns></returns>
            public static Property CreatePropertyFromString(string definition, string forceType = null)
            {

                //split the full definition string into name and value (name:value -> name, value)
                string[] s = definition.Split(setOperator);
                string propertyDefinition = s[0];
                string propertyValueDefinition = "";
                if (s.Length >= 2)
                {
                    propertyValueDefinition = s[1];
                }

                //if the property has a type, assign the type. Otherwise set to default
                string type;
                string name;
                if (propertyDefinition.Contains(typeNameSeperator.ToString()))
                {
                    string[] p = propertyDefinition.Split(typeNameSeperator);
                    type = p[0];
                    name = p[1];
                }
                else
                {
                    type = forceType==null? defaultParameterType : forceType;
                    name = propertyDefinition;
                }

                Property property = new Property(name, type);

                //if the first character is @ then we wait to handle the reference until after all other files are loaded
                if (propertyValueDefinition.Length > 0 && propertyValueDefinition[0] == getValueMarker)
				{
                    AddReference(property, propertyValueDefinition.Substring(1));
				}
                else
				{
                    // Parse the value definition into a string array to be used by the property ("value1,value2" -> ["value1", "value2"])
                    property.SetValue(propertyValueDefinition.Split(','));
                    property.CalculateOtherValueFormsFromStringValue();
				}

                return property;
            }
            public static void AssignFromReference (Reference reference)
			{
                Property referred = root.GetProperty(reference.path.Split(stackSeperator));
                if (referred == null) { throw new System.InvalidOperationException($"Reference target {reference.path} not found"); }
                reference.property.SetValue(referred.ValueAsStringArray);
                reference.property.CalculateOtherValueFormsFromStringValue();
			}
        }
    }
}