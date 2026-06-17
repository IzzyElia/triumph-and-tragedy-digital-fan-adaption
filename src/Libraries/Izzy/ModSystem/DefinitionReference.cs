using System;
using System.Runtime.Serialization;

namespace TT2026.libraries.Izzy.ModSystem
{
    [Serializable]
    public class DefinitionReference <T> where T : DefinitionType, new()
    {
        private string _name;
        [NonSerialized]
        private T _definition;
        
        public T Definition
        {
            get
            {
                return _definition;
            }
        }

        public string Name => _name;

        
        public DefinitionReference(DefinitionReference<T> definition) : this(definition._name)
        {
            
        }
        public DefinitionReference(string name)
        {
            this._name = name;
            ReloadDefinition();
            ModHandler.OnModStateChanged += ReloadDefinition;
        }

        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            ReloadDefinition();
        }

        void ReloadDefinition()
        {
            DefinitionCollection<T> definitionCollection = DefinitionCollectionMetadata.GetDefinitionCollection<T>();
            _definition = definitionCollection.Get(_name);
        }
    }
}