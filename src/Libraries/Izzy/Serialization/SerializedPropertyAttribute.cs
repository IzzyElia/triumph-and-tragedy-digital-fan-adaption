using System;

namespace TT2026.libraries.Izzy.Serialization
{
    /// <summary>
    /// Applies a custom name to be used by the field in the serialized file
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SerializedPropertyAttribute : Attribute
    {
        public SerializedPropertyAttribute()
        {
        }
    }
}