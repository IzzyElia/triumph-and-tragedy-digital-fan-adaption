using System.Collections.Generic;
using System.Reflection;

namespace IzzysConsole.Utils
{
    public static class AssemblyUtils
    {
        public static MemberInfo[] GetAllMembersWithAttribute<Attribute>(IReadOnlyCollection<Assembly> assemblies, BindingFlags bindingFlags = BindingFlags.Default)
        {
            List<MemberInfo> found = new List<MemberInfo>();
            foreach (Assembly assembly in assemblies)
            {
                foreach (System.Type type in assembly.GetTypes())
                {
                    if (System.Attribute.IsDefined(type, typeof(Attribute))) { found.Add(type.GetTypeInfo()); }
                    foreach (MemberInfo methodInfo in type.GetMembers(bindingFlags))
                    {
                        if (System.Attribute.IsDefined(methodInfo, typeof(Attribute))) { found.Add(methodInfo); }
                    }
                }
            }
            return found.ToArray();
        }
    }
}