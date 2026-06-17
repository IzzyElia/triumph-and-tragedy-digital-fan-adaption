namespace TT2026.libraries.Izzy.ModSystem
{
    public struct NameTypePair
    {
        public string name { get; private set; }
        public string type { get; private set; }
        public NameTypePair(string name, string type)
        {
            this.name = name;
            this.type = type;
        }
        public NameTypePair(string packedString)
        {
            string[] nameTypeArray = packedString.Split(':');
            if (nameTypeArray.Length == 1)
            {
                this.name = nameTypeArray[0];
                this.type = null;
            }
            else if (nameTypeArray.Length == 2)
            {
                this.name = nameTypeArray[1];
                this.type = nameTypeArray[0];
            }
            else { throw new System.ArgumentException($"Unable to unpack string '{packedString}' to a NameTypePair"); }
        }

        public override string ToString()
        {
            return $"{type}:{name}";
        }
    }
}