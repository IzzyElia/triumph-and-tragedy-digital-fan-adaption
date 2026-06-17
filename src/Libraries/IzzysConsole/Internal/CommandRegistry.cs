using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using TT2026.libraries.IzzysConsole.API;
using TT2026.libraries.IzzysConsole.API.Exceptions;
using TT2026.libraries.IzzysConsole.Internal.Exceptions;
using TT2026.libraries.IzzysConsole.Utilitity;

namespace TT2026.libraries.IzzysConsole.Internal
{
    public static class CommandRegistry
    {
        [ConsoleCommand(
            name: "help", 
            helpInfo:"Displays all root commands, or information about a specific command"
            )]
        static void Help(string command = null)
        {
            if (command == null)
            {
                ConsoleManager.Log($"The following commands are loaded -");
                foreach (ICommand rootCommand in GetAllCommandsInScope(_rootScope))
                    ConsoleManager.Log(' '+rootCommand.commandName);
            }
            else
            {
                (ICommand commandObj, string[] parameters, object scope) = GetCommand_Parameters_AndScopeOfCommandString(command);
                if (commandObj != null)
                {
                    ConsoleManager.Log(commandObj.format, color: "FFFCD1");
                    if (commandObj.helpInfo != null) 
                        ConsoleManager.Log(commandObj.helpInfo);
                }
                else
                    ConsoleManager.Log($"No command by the name '{command}'");
            }
        }
        [ConsoleCommand(
            name: "commands", 
            helpInfo:"Displays all valid commands in a scope (defaults to root scope)", 
            trailingFinalParameter: true // By setting trailingFinalParameter to true, the entire string entered by the user is treated as a single parameter value
            )]
        static void ListCommandsInScope(string scope = null)
        {
            if (scope == null)
            {
                foreach (System.Type type in _commandsByType.Keys)
                {
                    ConsoleManager.Log($"{type.Name}:");
                    foreach (ICommand command in _commandsByType.Get(type))
                        ConsoleManager.Log($"\t{command.format}");
                }
            }
            else
            {
                foreach (ICommand rootCommand in GetAllCommandsInScope(GetScopeOfCommandString(scope, includeFinal: true)))
                {
                    ConsoleManager.Log(' ' + rootCommand.format);
                }
            }

        }

        static object _rootScope = new Root();
        static readonly HashSet<Assembly> _loadedAssemblies = new HashSet<Assembly>();
        static readonly HashsetDictionary<System.Type, ICommand> _commandsByType = new HashsetDictionary<System.Type, ICommand>();
        static readonly Dictionary<System.Type, IParameterConverter> _parameterConverterMap = new Dictionary<System.Type, IParameterConverter>();
        static readonly List<string> tList = new List<string>();

        //Multithreading support
        static Task _loadingTask;
        static List<Assembly> _assemblyLoadingQueue = new List<Assembly>();
        public static bool activelyLoading => _loadingTask != null && _loadingTask.Status == TaskStatus.Running;
        
        static CommandRegistry()
        {
            //
        }

        private static bool _hasStartedInitialization = false;
        public static void Initialize()
        {
            _hasStartedInitialization = true;
            LoadDataFromAssemblies(multithread:true, System.AppDomain.CurrentDomain.GetAssemblies().Where(x => !Config.IsAssemblyExcluded(x)).ToArray());
            Config.CleanupAssemblyBlacklist();
        }
        public static IParameterConverter GetParameterConverterForType(System.Type type) => _parameterConverterMap.GetValueOrDefault(type, null);
        public static void LoadDataFromAssemblies(bool multithread, params Assembly[] assemblies)
        {
            foreach (Assembly assembly in assemblies)
                if (!_loadedAssemblies.Contains(assembly))
                {
                    _loadedAssemblies.Add(assembly);
                    _assemblyLoadingQueue.Add(assembly);
                }

            if (multithread)
            {
                if (!activelyLoading)
                {
                    _loadingTask = new Task(LoadDataFromUnloadedAssemblies);
                    _loadingTask.Start();
                }
            }
            else
            {
                LoadDataFromUnloadedAssemblies();
            }

        }
        static void LoadDataFromUnloadedAssemblies ()
        {
            while (_assemblyLoadingQueue.Count > 0)
            {
                Assembly[] assemblies;
                lock (_assemblyLoadingQueue)
                {
                    assemblies = _assemblyLoadingQueue.ToArray();
                    _assemblyLoadingQueue.Clear();
                }
                LoadParameterConvertersFromAssemblies(assemblies);
                LoadConsoleCommandsFromAssemblies(assemblies);
            }
        }
        static void LoadParameterConvertersFromAssemblies (IReadOnlyCollection<Assembly> assemblies)
        {
            foreach (TypeInfo memberInfo in AssemblyUtils.GetAllMembersWithAttribute<ParameterConverterAttribute>(assemblies: assemblies, bindingFlags: BindingFlags.Public | BindingFlags.NonPublic))
            {
                try
                {
                    ParameterConverterAttribute converterMetadata = memberInfo.GetCustomAttribute<ParameterConverterAttribute>();
                    TypeInfo type = memberInfo as TypeInfo;
                    if (!typeof(IParameterConverter).IsAssignableFrom(type)) throw new ParameterConverterDoesNotDeriveException(type);
                    if (type.ContainsGenericParameters) throw new ParameterConverterContainsGenericParametersException(type);
                    if (_parameterConverterMap.ContainsKey(converterMetadata.associatedType)) throw new ParameterConverterCompetitionException(_parameterConverterMap[converterMetadata.associatedType].GetType(), type.AsType(), converterMetadata.associatedType);
                    try
                    {
                        IParameterConverter converter = (IParameterConverter)System.Activator.CreateInstance(type.AsType());
                        _parameterConverterMap.Add(converterMetadata.associatedType, converter);
                    }
                    catch (System.MissingMethodException)
                    {
                        throw new ParameterConverterDoesNotContainParameterlessConstructorException(type.AsType());
                    }
                }
                catch (ParameterConverterDefinitionException error)
                {
                    GD.PrintErr($"Unable to load the ParameterConverter {memberInfo.Name}\n{error.Message}");
                }
                catch (System.Exception error)
                {
                    GD.PrintErr(error);
                }
            }
        }
        static void LoadConsoleCommandsFromAssemblies(IReadOnlyCollection<Assembly> assemblies)
        {
            foreach (MemberInfo memberInfo in AssemblyUtils.GetAllMembersWithAttribute<ConsoleCommandAttribute>(assemblies: assemblies, bindingFlags: BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance /*Instance methods are not supported, but we check for them anyways so the appropiate error can be thrown*/))
            {
                ConsoleCommandAttribute commandMetadata = memberInfo.GetCustomAttribute<ConsoleCommandAttribute>();
                ICommand command;
                System.Type scopeType;
                try
                {
                    if (commandMetadata.commandName == null) throw new ConsoleCommandFormatException($"Console command name cannot be null");
                    if (commandMetadata.commandName.Length == 0) throw new ConsoleCommandFormatException($"Console command name cannot be empty");
                    if (commandMetadata.commandName.Contains(' ')) throw new ConsoleCommandFormatException($"Console command name cannot contain spaces");
                    if (commandMetadata.commandName.Contains('.')) throw new ConsoleCommandFormatException($"Console command name cannot contain periods");
                    if (Config.IsScopingNullifier(commandMetadata.commandName[0])) throw new ConsoleCommandFormatException($"Console command name cannot start with the character {commandMetadata.commandName[0]}");

                    command = CreateCommandFromMember(memberInfo, commandMetadata);
                    if (command.isScopedCommand) scopeType = memberInfo.DeclaringType;
                    else scopeType = _rootScope.GetType();
                    if (_commandsByType.Contains(scopeType, command))
                    {
                        ICommand existingCommand = GetCommandInScopeType(command.commandName, scopeType);
                        throw new DuplicateConsoleCommandsInSameScopeException(existingCommand, command);
                    }

                }
                catch (ConsoleCommandDefinitionException error)
                {
                    GD.PushWarning($"Unable to create console command '{commandMetadata.commandName}'\n{error.Message}");
                    continue;
                }
                catch (DuplicateConsoleCommandsInSameScopeException error)
                {
                    GD.PushWarning($"Unable to create console command '{commandMetadata.commandName}'\n{error.Message}");
                    continue;
                }
                catch (System.Exception error)
                {
                    GD.PushError(error);
                    continue;
                }
                _commandsByType.Add(scopeType, command);
                GD.Print($"Loaded command '{command.commandName}'");
            }
        }
        static ICommand CreateCommandFromMember (MemberInfo memberInfo, ConsoleCommandAttribute commandMetadata)
        {
            if (memberInfo is MethodInfo) return new MethodCommand((MethodInfo)memberInfo, commandMetadata);
            else if (memberInfo is PropertyInfo) return new PropertyCommand((PropertyInfo)memberInfo, commandMetadata);
            else throw new ConsoleCommandInvalidDefinitionTypeException($"{memberInfo.Name} is not a method or property");
        }
        static (string command, string[] parameters) SeperateCommandFromParameters(string combinedCommandString)
        {
            if (combinedCommandString == null) throw new System.InvalidOperationException($"Input string for SeperateCommandFromParameters cannot be null");
            if (combinedCommandString.Length == 0) throw new System.InvalidOperationException($"Input string for SeperateCommandFromParameters cannot be empty");

            string[] splitCommandString = combinedCommandString.Split(' ');
            string commandName = splitCommandString[0];
            string[] parameters = new string[splitCommandString.Length - 1];
            for (int i = 1; i < splitCommandString.Length; i++)
            {
                parameters[i - 1] = splitCommandString[i];
            }
            return (commandName, parameters);
        }
        static ICommand GetCommandInScope(string commandName, object scope) => GetCommandInScopeType(commandName, scope.GetType());
        static ICommand GetCommandInScopeType(string commandName, System.Type scopeType)
        {
            foreach (ICommand command in _commandsByType.Get(scopeType))
            {
                if (command.commandName == commandName) return command;
            }
            return null;
        }
        static IReadOnlyCollection<ICommand> GetAllCommandsInScope(object scope)
        {
            if (scope == null) return new ICommand[0];
            return _commandsByType.Get(scope.GetType());
        }
        static string[] SplitCommandPath (string commandPath)
        {
            tList.Clear();
            string current = string.Empty;
            for (int i = 0; i < commandPath.Length; i++)
            {
                char character = commandPath[i];
                if (character == '.')
                {
                    if (i == commandPath.Length - 1) continue;
                    else if (!Config.IsScopingNullifier(commandPath[i + 1]))
                    {
                        tList.Add(current);
                        current = string.Empty;
                        continue;
                    }
                    else
                    {
                        current += character;
                    }
                }
                else
                {
                    current += character;
                }
            }
            tList.Add(current);
            return tList.ToArray();
        }
        /// <summary>
        /// Takes a path such as 'rootcommand.subobject.command' and returns the current scope ([subobject] in this example)
        /// </summary>
        /// <param name="commandPath"></param>
        /// <returns>Returns null if the command doesn't lead to a valid scope. Otherwise returns the active scope</returns>
        static object GetScopeOfCommandString (string commandPath, bool includeFinal = false)
        {
            if (commandPath == null) return null;
            if (commandPath.Trim().Length == 0) return null;
            string[] splitCommandPath = SplitCommandPath(commandPath);
            string rootCommandName = splitCommandPath[0];
            object scope = _rootScope;
            int length = includeFinal ? splitCommandPath.Length : splitCommandPath.Length - 1;
            for (int i = 0; i < length; i++)
            {
                (string commandName, string[] parameters) = SeperateCommandFromParameters(splitCommandPath[i]);
                ICommand command = GetCommandInScope(commandName, scope);
                if (command == null) return null;
                scope = command.GetSubscope(parameters, scope);
                if (scope == null) return null;
            }
            return scope;
        }
        static (ICommand command, string[] parameters, object scope) GetCommand_Parameters_AndScopeOfCommandString (string commandPath)
        {
            object scope = GetScopeOfCommandString(commandPath);
            if (scope == null) return (null, null, null);
            string[] splitCommandPath = SplitCommandPath(commandPath);
            (string commandName, string[] parameters) = SeperateCommandFromParameters(splitCommandPath[splitCommandPath.Length - 1]);
            return (GetCommandInScope(commandName, scope), parameters, scope);
        }
        /// <summary>
        /// Attempts to execute the specified command string, returning a <see cref="ConsoleCommandResult"/> object with it's status.
        /// </summary>
        /// <param name="commandPath">The full path to the command</param>
        /// <remarks>
        /// In release builds, ALL exceptions will be caught. In debug builds, only <see cref="ConsoleCommandDefinitionException"/> and <see cref="ConsoleCommandUserErrorException"/> will be caught
        /// </remarks>
        public static object TryExecuteCommand(string commandPath)
        {
            if (!_hasStartedInitialization) GD.PrintErr("Console Command Registry not initialized. Initialize it by calling CommandRegistry.Initialize()");
            if (activelyLoading) _loadingTask.Wait(); // If we're still scanning assemblies, wait for the process to finish
            commandPath = commandPath.Trim();
            (ICommand command, string[] parameters, object scope) = GetCommand_Parameters_AndScopeOfCommandString(commandPath);
            
            if (command == null)
            {
                string[] splitCommandPath = SplitCommandPath(commandPath);
                string commandName = SeperateCommandFromParameters(splitCommandPath[splitCommandPath.Length - 1]).command;
                ConsoleManager.Log($"'{commandName}' is not a valid command", ConsoleManager.errorColor);
                return null;
            }
            object returnedObject;
            try
            {
                returnedObject = command.Execute(parameters, context: scope);
            }
            catch (ConsoleCommandDefinitionException err)
            {
                ConsoleManager.Log($"Error in command definition for command '{command.commandName}': {err.Message}", ConsoleManager.errorColor);
                GD.PrintErr($"Error in command definition for command '{command.commandName}': {err.Message}");
                return null;
            }
            catch (ConsoleCommandUserErrorException userError)
            {
                ConsoleManager.Log(userError.Message, ConsoleManager.errorColor);
                GD.PushWarning(userError.Message);
                return null;
            }
            /*
            catch (System.Exception unhandledException)
            {
                ConsoleManager.Log($"Unhandled exception attempting to execute command '{command.commandName}': \n{unhandledException.InnerException}\nSee error log for more details", ConsoleManager.errorColor);
                GD.PushWarning($"Unhandled exception attempting to execute command '{command.commandName}': \n{unhandledException.InnerException}\nSee error log for more details");
                return null;
            }
            */
            return returnedObject; // No error - Success!
        }
        static string ConvertParameterNameToDisplayName (string parameterName)
        {
            string output = "";
            for (int i = 0; i < parameterName.Length; i++)
            {
                char character = parameterName[i];
                if (i != 0)
                {
                    if (char.IsUpper(character))
                        output += ' ';
                    output += character;
                }
                else output += char.ToUpper(character);
            }
            return output;
        }

        class Root { } // This class just exists to reserve a spot in commandsByType

        // Command
        struct InvalidCommand : ICommand
        {
            public string commandName { get; private set; }
            public MemberInfo implimentingMember { get; private set; }
            public string format { get; private set; }
            public string helpInfo { get; private set; }
            public bool isScopedCommand { get; private set; }

            public object Execute(string[] userParameters, object context)
            {
                throw new System.InvalidOperationException($"The command {commandName} is improperly defined and was not loaded. See the unity warning log for more details");
            }
            public object GetSubscope(string[] userParameters, object context)
            {
                return null;
            }
            public InvalidCommand(ConsoleCommandAttribute metadata, MemberInfo member)
            {
                this.commandName = metadata.commandName;
                this.implimentingMember = member;
                this.helpInfo = metadata.helpInfo;
                this.format = metadata.commandName;
                if (member is MethodInfo) this.isScopedCommand = !((MethodInfo)member).IsStatic;
                else if (member is PropertyInfo)
                {
                    if (((PropertyInfo)member).GetMethod != null)
                    {
                        this.isScopedCommand = !(((PropertyInfo)member).GetMethod.IsStatic);
                    }
                    else if (((PropertyInfo)member).SetMethod != null)
                    {
                        this.isScopedCommand = !(((PropertyInfo)member).SetMethod.IsStatic);
                    }
                    else this.isScopedCommand = false;
                }
                else this.isScopedCommand = false;


            }
            public override int GetHashCode() => commandName.GetHashCode();
            public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();
        }
        struct MethodCommand : ICommand
        {
            public ConsoleCommandAttribute commandMetadata { get; private set; }
            public MethodInfo commandMethod { get; private set; }
            public IParameterConverter[] parameterConverters { get; private set; }
            public System.Type[] parameterTypes { get; private set; }
            public object[] parameterDefaults { get; private set; }
            public int requiredParametersCount { get; private set; }
            public bool isScopedCommand { get; private set; }
            public string[] parameterKeys { get; private set; }
            public string helpInfo => commandMetadata.helpInfo;
            public string commandName => commandMetadata.commandName;
            public string format
            {
                get
                {
                    string formatString = commandMetadata.commandName;
                    for (int i = 0; i < parameterKeys.Length; i++)
                    {
                        formatString += $" [{parameterKeys[i]}]";
                    }
                    return formatString;
                }
            }
            bool trailingFinalParameter => commandMetadata.trailingFinalParameter;
            int parameterCount;

            MemberInfo ICommand.implimentingMember => commandMethod;

            public MethodCommand(MethodInfo commandMethod, ConsoleCommandAttribute commandMetadata)
            {
                if (commandMethod.ContainsGenericParameters) throw new ConsoleCommandContainsGenericParametersException();
                if (commandMethod.IsStatic && commandMethod.DeclaringType.ContainsGenericParameters) throw new GlobalConsoleCommandInGenericClassException();
                this.commandMetadata = commandMetadata;
                this.commandMethod = commandMethod;
                this.isScopedCommand = !commandMethod.IsStatic;

                ParameterInfo[] parameterInfos = commandMethod.GetParameters();
                this.parameterTypes = new System.Type[parameterInfos.Length];
                this.parameterConverters = new IParameterConverter[parameterInfos.Length];
                this.parameterCount = parameterInfos.Length;
                this.parameterDefaults = new object[parameterCount];
                this.requiredParametersCount = 0;
                for (int i = 0; i < parameterCount; i++)
                {
                    if (parameterInfos[i].HasDefaultValue)
                    {
                        parameterDefaults[i] = parameterInfos[i].DefaultValue;
                    }
                    else
                    {
                        parameterDefaults[i] = null;
                        requiredParametersCount++;
                    }
                }

                // For each parameter..
                this.parameterKeys = new string[parameterInfos.Length];
                for (int i = 0; i < parameterInfos.Length; i++)
                {
                    // Append the commands help info
                    if (i < commandMetadata.parameterKeys.Length)
                        parameterKeys[i] = commandMetadata.parameterKeys[i];
                    else
                        parameterKeys[i] = ConvertParameterNameToDisplayName(parameterInfos[i].Name);

                    // Assign a parameter converter
                    parameterTypes[i] = parameterInfos[i].ParameterType;
                    IParameterConverter parameterConverter = GetParameterConverterForType(parameterTypes[i]);
                    if (parameterConverter == null) throw new ConsoleCommandNoConverterForParameterException(parameterTypes[i]);
                    parameterConverters[i] = parameterConverter;

                }
            }
            public object Execute(string[] userParameters, object context)
            {
                if (userParameters.Length < requiredParametersCount) throw new ConsoleCommandInsufficientParameterException(commandName, requiredParametersCount, parameterKeys);
                object[] convertedUserParameters = new object[parameterCount];
                if (trailingFinalParameter && userParameters.Length > parameterCount)
                {
                    for (int i = parameterCount; i < userParameters.Length; i++)
                        userParameters[parameterCount - 1] += $" {userParameters[i]}";
                }
                for (int i = 0; i < parameterCount; i++)
                {
                    if (i < userParameters.Length)
                    {
                        convertedUserParameters[i] = parameterConverters[i].Convert(userParameters[i]);
                        if (convertedUserParameters[i] == null) throw new ConsoleCommandParameterParseException("Value", parameterTypes[i]);
                        if (convertedUserParameters[i].GetType() != parameterTypes[i]) throw new ParameterConverterReturnedWrongTypeException(parameterConverters[i], parameterTypes[i], convertedUserParameters[i].GetType());
                    }
                    else convertedUserParameters[i] = parameterDefaults[i];
                }

                object result = GetMethodConvertedToGenericIfApplicable(context).Invoke(context, convertedUserParameters);
                return result;
            }
            public object GetSubscope(string[] userParameters, object context)
            {
                if (!commandMetadata.scopingAllowed) return null;
                object returnedObject;
                try
                {
                    returnedObject = Execute(userParameters, context);
                }
                catch (ConsoleCommandDefinitionException err)
                {
                    ConsoleManager.Log($"Error in command definition for command '{commandName}': {err.Message}", ConsoleManager.errorColor);
                    GD.PushWarning($"Error in command definition for command '{commandName}': {err.Message}");
                    return null;
                }
                catch (ConsoleCommandUserErrorException userError)
                {
                    ConsoleManager.Log(userError.Message, ConsoleManager.errorColor);
                    GD.PushWarning(userError.Message);
                    return null;
                }
                return returnedObject;
            }
            public override int GetHashCode() => commandName.GetHashCode();
            public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();
            MethodInfo GetMethodConvertedToGenericIfApplicable (object context)
            {
                if (commandMethod.DeclaringType.ContainsGenericParameters)
                {
                    System.Type[] generics = context.GetType().GenericTypeArguments;
                    return commandMethod.MakeGenericMethod(generics);
                }
                else
                {
                    return commandMethod;
                }
            }
        }
        struct PropertyCommand : ICommand
        {
            public ConsoleCommandAttribute commandMetadata { get; private set; }
            public MemberInfo implimentingMember => property;
            public PropertyInfo property { get; private set; }
            public MethodInfo getMethod { get; private set; }
            public MethodInfo setMethod { get; private set; }
            public IParameterConverter parameterConverter { get; private set; }
            public bool isScopedCommand { get; private set; }
            public string format { get; private set; }
            public string commandName => commandMetadata.commandName;
            public string helpInfo => commandMetadata.helpInfo;
            bool trailingFinalParameter => commandMetadata.trailingFinalParameter;
            public PropertyCommand(PropertyInfo commandProperty, ConsoleCommandAttribute commandMetadata)
            {
                this.commandMetadata = commandMetadata;
                this.property = commandProperty;
                this.getMethod = commandProperty.GetMethod;
                this.setMethod = commandProperty.SetMethod;
                this.format = commandMetadata.commandName;
                this.parameterConverter = setMethod == null ? null : GetParameterConverterForType(commandProperty.PropertyType);
                if (this.parameterConverter == null && setMethod != null) throw new ConsoleCommandNoConverterForParameterException(commandProperty.PropertyType);
                if (commandProperty.SetMethod != null)
                {
                    if (commandProperty.SetMethod.ContainsGenericParameters) throw new ConsoleCommandContainsGenericParametersException();
                    if (commandProperty.SetMethod.IsStatic && commandProperty.DeclaringType.ContainsGenericParameters) throw new GlobalConsoleCommandInGenericClassException();
                    this.isScopedCommand = !(commandProperty.SetMethod.IsStatic);
                    this.format += commandMetadata.parameterKeys.Length > 0 ? $" [{commandMetadata.parameterKeys[0]}]" : $" [Value]";
                }
                else if (commandProperty.GetMethod != null)
                {
                    if (commandProperty.GetMethod.ContainsGenericParameters) throw new ConsoleCommandContainsGenericParametersException();
                    if (commandProperty.GetMethod.IsStatic && commandProperty.DeclaringType.ContainsGenericParameters) throw new GlobalConsoleCommandInGenericClassException();
                    this.isScopedCommand = !(commandProperty.GetMethod.IsStatic);
                }
                else this.isScopedCommand = false;
            }
            public object Execute(string[] userParameters, object context)
            {
                if (trailingFinalParameter && userParameters.Length > 1)
                {
                    for (int i = 1; i < userParameters.Length; i++)
                        userParameters[0] += $" {userParameters[i]}";
                }
                if (userParameters.Length == 0 || setMethod == null)
                {
                    if (getMethod != null)
                    {
                        MethodInfo method;
                        if (implimentingMember.DeclaringType.ContainsGenericParameters)
                        {
                            System.Type[] generics = context.GetType().GenericTypeArguments;
                            method = getMethod.MakeGenericMethod(generics);
                        }
                        else
                        {
                            method = getMethod;
                        }
                        object result = method.Invoke(context, new object[0]);
                        return result;
                    }
                    else
                        return null;
                }
                else
                {
                    MethodInfo method;
                    if (implimentingMember.DeclaringType.ContainsGenericParameters)
                    {
                        System.Type[] generics = context.GetType().GenericTypeArguments;
                        method = setMethod.MakeGenericMethod(generics);
                    }
                    else
                    {
                        method = setMethod;
                    }
                    object convertedUserParameter = parameterConverter.Convert(userParameters[0]);
                    if (convertedUserParameter == null) throw new ConsoleCommandParameterParseException("Value", property.PropertyType);
                    if (convertedUserParameter.GetType() != property.PropertyType) throw new ParameterConverterReturnedWrongTypeException(parameterConverter, property.PropertyType, convertedUserParameter.GetType());
                    method.Invoke(context, new object[1] { convertedUserParameter });
                    return null;
                }
            }
            public object GetSubscope(string[] userParameters, object context)
            {
                if (!commandMetadata.scopingAllowed) return null;
                if (getMethod == null) return null;
                MethodInfo method;
                if (implimentingMember.DeclaringType.ContainsGenericParameters)
                {
                    System.Type[] generics = context.GetType().GenericTypeArguments;
                    method = getMethod.MakeGenericMethod(generics);
                }
                else
                {
                    method = getMethod;
                }

                object returnedObject;
                try
                {
                    returnedObject = Execute(userParameters, context);
                }
                catch (ConsoleCommandDefinitionException err)
                {
                    ConsoleManager.Log($"Error in command definition for command '{commandName}': {err.Message}", ConsoleManager.errorColor);
                    GD.PushWarning($"Error in command definition for command '{commandName}': {err.Message}");
                    return null;
                }
                catch (ConsoleCommandUserErrorException userError)
                {
                    ConsoleManager.Log(userError.Message, ConsoleManager.errorColor);
                    GD.PushWarning(userError.Message);
                    return null;
                }
                return returnedObject;
            }
            public override int GetHashCode() => commandName.GetHashCode();
            public override bool Equals(object obj) => GetHashCode() == obj.GetHashCode();
        }
    }
}