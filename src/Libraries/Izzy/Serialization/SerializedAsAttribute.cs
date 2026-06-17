using System;

namespace TT2026.libraries.Izzy.Serialization
{
    /// <summary>
    /// Applies a custom name to be used by the field in the serialized file
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SerializedAsAttribute : Attribute
    {
        public string SerializedShorthand { get; private set; }
        public SerializedAsAttribute(string serializedShorthand)
        {
            this.SerializedShorthand = serializedShorthand;
        }
    }
}