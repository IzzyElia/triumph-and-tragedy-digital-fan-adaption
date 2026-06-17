using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace TT2026.libraries.Izzy.ForcedInitialization
{
    public sealed class ForceInitializer
	{
		public static void InitializeUninitializedTypes()
		{
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
			IEnumerable<Type> types = new List<Type>();
			foreach (Assembly assembly in assemblies)
			{
				types = types.Concat(assembly.GetTypes());
			}
			foreach (Type type in types.Where(x => Attribute.IsDefined(x, typeof(ForceInitializeAttribute))))
			{
				InitializeClassIfNotYet(type);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (type.GetCustomAttribute<ForceInitializeAttribute>().includeDerived)
				{
					foreach (Type subType in types.Where(x => x.IsAssignableFrom(type)))
					{
						DynamicLogger.Log($"Initializing type {subType}");
						InitializeClassIfNotYet(subType);
						
					}
				}
#pragma warning restore CS8602 // Dereference of a possibly null reference.
			}
		}
		static void InitializeClassIfNotYet(Type type)
		{
			if (!initializedTypes.Contains(type))
			{
				RuntimeHelpers.RunClassConstructor(type.TypeHandle);
				initializedTypes.Add(type);
			}
		}
		public static HashSet<Type> initializedTypes = new HashSet<Type>();
	}
}