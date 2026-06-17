using System;

namespace TT2026.libraries.Izzy.ModSystem
{
    public abstract class DefinitionType
    {
        public string Name { get; private set; }
        public int id { get; private set; }
        public DefinitionType() { }
        public void DoSetup(GameData definition, int id)
        {
            
            this.id = id;
            this.Name = definition.Name;
            Setup(definition);
        }

        public void SetupFallback()
        {
            id = -2;
            Fallback();
        }
        protected abstract void Fallback();
        protected abstract void Setup(GameData definition);
        
        protected class InvalidDefinitionException : Exception
        {
            public InvalidDefinitionException(string message)
                : base(message)
            {
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}