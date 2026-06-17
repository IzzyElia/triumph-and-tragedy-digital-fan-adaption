using System;

namespace TT2026.libraries.Izzy.UnitTesting
{
    [AttributeUsage(AttributeTargets.Method)]
    public class TestAttribute : Attribute
    {
        public TestAttribute()
        {
        }
    }
}