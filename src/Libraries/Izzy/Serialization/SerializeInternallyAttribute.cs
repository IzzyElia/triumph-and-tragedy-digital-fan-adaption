using System;

namespace TT2026.libraries.Izzy.Serialization
{
    /// <summary>
    /// Specifies that a field or property that is a reference type should be serialized as if it were a value type.
    /// The referenced type will not be serialized in the master list of referenced objects but rather as a child of
    /// the referencer
    ///
    /// To avoid bugs arising from the accidental duplication of objects on deserialization, an error will be
    /// thrown if another serialized object references the target. Ie this attribute should only be applied to
    /// references of objects that are exclusive to the referencer
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    sealed class SerializeInternallyAttribute : Attribute
    {
        public SerializeInternallyAttribute()
        {
            
        }
    }
}