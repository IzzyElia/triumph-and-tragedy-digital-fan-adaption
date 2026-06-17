using TT2026.libraries.Izzy.ForcedInitialization;

namespace TT2026.libraries.Izzy.ModSystem
{
    [ForceInitialize]
    public class ExampleType : DefinitionType
    {
        // The type creates its own definition collection in the static constructor.
        // The parameter in the DefinitionCollection constructor maps it to a type in the config file
        private static DefinitionCollection<ExampleType> _definitionCollection;
        static ExampleType()
        {
            _definitionCollection = new DefinitionCollection<ExampleType>("TYPE NAME HERE");
        }
    
        protected override void Fallback()
        {
            // Define the fallback type here
        }

        protected override void Setup(GameData definition)
        {
            // Define the type from a given definition here
        }
    }
}

