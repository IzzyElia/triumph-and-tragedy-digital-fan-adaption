using System;
using System.Collections.Generic;
using System.IO;

namespace TT2026.libraries.Izzy.ModSystem
{
    /// <summary>
    /// On ModHandler.Reset -> The game data is about to be (re)loaded
    /// On ModHandler.Load -> Load game data from [directory]
    /// On ModHandler.Lock -> The game data has changed. Refresh anything that relies on it
    /// </summary>
    public static class ModHandler
    {
        private static string modsPath = null;

        /// <summary>
        /// Triggered during the cleanup before (re)loading mods
        /// </summary>
        public static event System.Action OnUnloadingMods;
        public static event System.Action<DirectoryInfo> OnLoadMod;
        /// <summary>
        /// Triggered once new mod data has been loaded
        /// </summary>
        public static event System.Action RebuildDynamicDefinitions;
        public static event System.Action OnModStateChanged;
        public static List<string> mods = new List<string>();
        public static bool Loaded { get; private set; } = false;
        /// <summary>
        /// Assuming all classes that use it are setup correctly, calling this should reload the entire mod system
        /// </summary>
        public static void ReloadMods()
        {
            OnUnloadingMods?.Invoke();
            foreach (string mod in mods)
            {
                
                LoadMod(mod);
            }
            LockModState();
        }

        public static void SetModPath(string path)
        {
            modsPath = path;
        }
        /// <summary>
        /// Adds a mod to tbe list of mods to be loaded when Build() is run
        /// </summary>
        /// <param name="The mods directory name"></param>
        /// <returns></returns>
        public static bool TryAddMod(string name)
        {
            if (mods.Contains(name))
                return false;

            if (!new DirectoryInfo(Path.Combine(modsPath, name)).Exists)
            {
                DynamicLogger.LogWarning($"No mod found at {Path.Combine(modsPath, name)}");
                return false;
            }

            mods.Add(name);
            return true;
        }
        /// <summary>
        /// Loads a mod
        /// </summary>
        /// <param name="The mods directory name"></param>
        static void LoadMod(string name)
        {
            DynamicLogger.Log($"Loading mod {name}");
            if (modsPath == null)
                throw new InvalidOperationException(
                    "Mod path has not been set. You can set it by calling ModHandler.SetModPath()");
            string path = Path.Combine(modsPath, name);
            DynamicLogger.Log($"Loading mod {name} from '{path}'");
            DirectoryInfo directory = new DirectoryInfo(path);
            if (directory.Exists)
            {
                OnLoadMod?.Invoke(directory);
            }
            else
            {
                DynamicLogger.LogWarning($"Failed to load mod at {path}");
            }
        }
        /// <summary>
        /// Run when all mods have finished loading. Fires the event onLock,
        /// which may be useful for having certain elements initiate only after mods have loaded,
        /// or reloading elements after a rebuild.
        /// </summary>
        static void LockModState()
        {
            Loaded = true;
            RebuildDynamicDefinitions?.Invoke();
            OnModStateChanged?.Invoke();
            DynamicLogger.Log("Mods State Applied");
        }
    }
}
