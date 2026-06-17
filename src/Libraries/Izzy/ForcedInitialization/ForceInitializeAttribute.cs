using System;

namespace TT2026.libraries.Izzy.ForcedInitialization
{
    /// <summary>
    /// Calling ForceInitializer.InitializeUninitializedTypes() will force this class to run its static constructor if it hasn't already
    /// </summary>
    public class ForceInitializeAttribute : Attribute
	{
		public bool includeDerived { get; private set; }
		public ForceInitializeAttribute() : this (false) { }
		public ForceInitializeAttribute(bool includeDerived)
		{
			this.includeDerived = includeDerived;
		}
	}
}