using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace TT2026.libraries.Izzy.ModSystem
{
	[Serializable]
    public class DataNode
    {
        string _name;
        public string Name
        {
            get { return _name; }
            protected set { _name = value; }
        }
        string _type;
        public string Type
        {
            get { return _type; }
            protected set { _type = value; }
        }
        public NameTypePair NameAndType
        {
            get { return new NameTypePair(this.Name, this.Type); }
            set { this.Name = value.name; this.Type = value.type; }
        }

        protected List<DataNode> children;
        public DataNode[] Children
		{
            get { return children.ToArray(); }
		}
        protected Property.Collection properties;
        /// Not serialized
        [NonSerialized] DataNode _parent; //recreated by OnDeserialized
        public DataNode Parent
		{
            get { return _parent; }
            set { value.AddChild(this); } //AddChild() properly sets up the parent/child relationship
		}

        //Serialization
        /*
        protected virtual void Serialize (SerializationInfo info, StreamingContext context)
		{
            info.AddValue("Name", name);
            info.AddValue("Type", type);
            info.AddValue("Children", children);
            info.AddValue("Properties", properties);
        }
        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
            Serialize(info, context);
		}
        protected DataNode (SerializationInfo info, StreamingContext context)
		{
            name = info.GetString("Name");
            type = info.GetString("Type");
            properties = (Property.Collection)info.GetValue("Properties", typeof(Property.Collection));
            children = (List<DataNode>)info.GetValue("Children", typeof(List<DataNode>));
        }
        */
        [OnDeserialized] void OnDeserialized ()
		{
            // Rebuild the parent field for all children
            foreach(DataNode child in children)
			{
                child._parent = this;
			}
		}
        public DataNode(string name = null, string type = null, DataNode parent = null)
        {
            children = new List<DataNode>();
            properties = new Property.Collection();
            this.Name = name;
            this.Type = type;
            if (parent != null) { this.Parent = parent; }
        }
        public DataNode() : this(null, null, null) { }
        public DataNode(NameTypePair nameType, DataNode parent = null) : this(nameType.name, nameType.type, parent) { }
        public void AddProperty(Property property)
        {
            this.properties.Add(property);
        }
        public void AddProperties(Property[] properties)
        {
            foreach(Property property in properties)
			{
                this.properties.Add(property);
            }
        }
        public Property[] Properties
        {
            get => properties.ToArray();
        }
        public Property[] GetPropertiesOfType(string type)
        {
            List<Property> foundProperties = new List<Property>();
            foreach (Property property in properties)
            {
                if (property.type == type) { foundProperties.Add(property); }
            }
            return foundProperties.ToArray();
        }
        public string[] GetPropertyValues(string name)
        {
            foreach (Property property in properties)
            {
                if (property.name == name)
                {
                    return property.ValueAsStringArray;
                }
            }
            return new string[0];
        }
        public Property GetProperty(string name, string type = null)
        {
            foreach (Property property in properties)
            {
                if (property.name == name && (type == null || property.type == null || property.type == type)) { return property; }
            }
            return null;
        }
        public Property GetProperty(string[] path, string type = null)
        {
            if (path.Length == 1) { return GetProperty(path[0]); }
            DataNode node = GetChild(PathExceptLast(path));
            if (node == null) { return null; }
            else { return node.GetProperty(path[path.Length - 1]); }
        }
        public string GetPropertyValueAsString(string name, string type = null, string fallback = "")
		{
            return GetPropertyValueAsString(new string[1] { name }, type, fallback);
		}
        public string GetPropertyValueAsString(string[] path, string type = null, string fallback = "")
        {
            string str = GetProperty(path, type)?.ValueAsString;
            return str == null ? fallback : str;
        }
        public string[] GetPropertyValueAsStringArray(string name, string type = null)
        {
            return GetPropertyValueAsStringArray(new string[1] { name }, type);
        }
        public string[] GetPropertyValueAsStringArray(string[] path, string type = null)
        {
            string[] array = GetProperty(path, type)?.ValueAsStringArray;
            return array == null ? new string[0] : array;
        }
        public float GetPropertyValueAsFloat(string name, string type = null, float fallback = 0)
        {
            return GetPropertyValueAsFloat(new string[] { name }, type, fallback);
        }
        public float GetPropertyValueAsFloat(string[] path, string type = null, float fallback = 0)
        {
            Property property = GetProperty(path, type);
            if (property == null) { return fallback; }
            else { return property.ValueAsFloatOrFallback(fallback); }
        }
        public float[] GetPropertyValueAsFloatList(string name, string type = null)
		{
            return GetPropertyValueAsFloatList(new string[1] { name }, type);
		}
        public float[] GetPropertyValueAsFloatList(string[] path, string type = null)
        {
            Property property = GetProperty(path, type);
            if (property == null) { return new float[0]; }
            else { return property.ValueAsFloatArray; }
        }
        public int GetPropertyValueAsInt(string name, string type = null, int fallback = 0)
        {
            return GetPropertyValueAsInt(new string[] { name }, type, fallback);
        }
        public int GetPropertyValueAsInt(string[] path, string type = null, int fallback = 0)
        {
            Property property = GetProperty(path, type);
            if (property == null) { return fallback; }
            else { return property.ValueAsIntOrFallback(fallback); }
        }
        public int[] GetPropertyValueAsIntArray(string name, string type = null)
        {
            return GetPropertyValueAsIntArray(new string[1] { name }, type);
        }
        public int[] GetPropertyValueAsIntArray(string[] path, string type = null)
        {
            Property property = GetProperty(path, type);
            if (property == null) { return new int[0]; }
            else { return property.ValueAsIntArray; }
        }
        public FloatColor GetPropertyValueAsColor(string name, string type = null, FloatColor fallback = default)
        {
            return GetPropertyValueAsColor(new string[] { name }, type, fallback);
        }
        public FloatColor GetPropertyValueAsColor(string[] path, string type = null, FloatColor fallback = default)
        {
            Property property = GetProperty(path, type);
            if (property == null) { return fallback; }
            else { return property.ValueAsColorOrFallback(fallback); }
        }
        /// <summary>
        /// Sets the specified property to the specified value, creating it if it does not exist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void SetProperty(string name, string value, string type = null, bool recalculateOtherValueTypes = false)
		{
            SetProperty(name, new string[1] { value }, type, recalculateOtherValueTypes);
		}
        public void SetProperty(string name, string[] value, string type = null, bool recalculateOtherValueTypes = false)
        {
            Property property = GetProperty(name, type);
            if (property == null)
			{
                AddProperty(new Property(name, type, value, recalculateOtherValueTypes));
			}
            else
			{
                property.SetValue(value);
                if (recalculateOtherValueTypes) property.CalculateOtherValueFormsFromStringValue();
			}
        }
        public void SetProperty(string[] path, string[] value, string type = null, bool recalculateOtherValueTypes = false)
        {
            if (path.Length == 1)
            {
                SetProperty(path[0], value, type, recalculateOtherValueTypes);
            }
            else
			{
                GetChild(path[0]).SetProperty((string[])path.Take(path.Length - 1), value, type, recalculateOtherValueTypes);
			}
        }
        public void SetProperty(string[] path, string value, string type = null, bool recalculateOtherValueTypes = false)
        {
            SetProperty(path, new string[1] { value }, type, recalculateOtherValueTypes);
        }
        public void SetProperty(Property property)
        {
            TryRemoveProperty(property.name, property.type);
            AddProperty(property);
        }
        /// <summary>
        /// Append an entry to a properties value, creating the property if it does not exist
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void AppendProperty (string name, string value, string type = null)
		{
            Property property = GetProperty(name, type);
            if (property == null) { properties.Add(new Property(name, type, value)); }
            else { property.AppendValue(value); }
        }
        /// <summary>
        /// Inverse of AppendProperty(). Removes a value from the property if applicable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public void RemoveFromProperty (string name, string value, string type = null)
		{
            Property property = GetProperty(name, type);
            if (property == null) { return; }
            else { property.RemoveValue(value); }
		}
        public bool TryRemoveProperty (string name, string type = null)
        {
            Property property = GetProperty(name, type);
            if (property == null)
            {
                return false;
            }
            else
            {
                properties.Remove(property);
                return true;
            }
        }
        public bool HasProperty(string name, string type = null)
        {
            return GetProperty(name, type) != null;
        }
        public bool HasProperty(string[] path, string type = null)
        {
            return GetProperty(path, type) != null;
        }
        public void AddChild(DataNode node)
        {
            children.Add(node);
            node._parent?.RemoveChild(node);
            node._parent = this;
        }
        public bool RemoveChild(DataNode child)
        {
            child.Parent = null;
            bool result = children.Remove(child);
            return result;
        }
        public DataNode GetChild(string[] path)
        {
            DataNode scope = this;
            for (int i = 0; i < path.Length; i++)
            {
                string s = path[i];
                scope = scope.GetChild(s);
                if (scope == null) { return null; }
            }
            return scope;
        }
        public DataNode GetChild(string name, string type = null)
        {
            foreach (DataNode child in children)
            {
                if (child.Name == name && (type == null || child.Type == null || child.Type == type)) { return child; }
            }
            return null;
        }
        public DataNode GetChild(NameTypePair nameType)
        {
            return GetChild(nameType.name, nameType.type);
        }
        public DataNode GetOrCreateChild (string name, string type = null)
		{
            DataNode child = GetChild(name, type);
            if (child == null) { child = new DataNode(name, type, this); AddChild(child); } 
            return child;

        }
        public DataNode GetOrCreateChild(string[] path, string type = null)
        {
            DataNode scope = this;
			for (int i = 0; i < path.Length; i++)
			{
                scope = GetOrCreateChild(path[i]);
			}
            return scope;
        }
        public List<DataNode> GetChildrenOfType(string type)
        {
            List<DataNode> children = new List<DataNode>();
            foreach (DataNode item in this.children)
            {
                if (item.Type == type) { children.Add(item); }
            }
            return children;
        }
        public void RegisterAs(string key)
        {
            if (namedNodes.ContainsKey(key))
            {
                namedNodes[key] = this;
            }
            else
            {
                namedNodes.Add(key, this);
            }
        }


        //Static


        static Dictionary<string, DataNode> namedNodes;
        public static DataNode NamedNode(string key)
        {
            if (namedNodes.TryGetValue(key, out DataNode node))
            {
                return node;
            }
            return null;
        }
        static DataNode()
		{
            namedNodes = new Dictionary<string, DataNode>();
        }


        //Internal

        private string[] PathExceptLast(string[] path)
        {
            string[] subPath = new string[path.Length - 1];
            for (int i = 0; i < path.Length - 1; i++)
            {
                subPath[i] = path[i];
            }
            return subPath;
        }
    }
    public static class DataNodeExtensions
	{
        public static string[] Path(this string str)
		{
            return str.Split('.');
		}
	}
}