using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

/*
namespace Izzy
{
    /// <summary>
    /// DEPRECIATED
    /// </summary>
    public class BuildInSerializerUtils
    {
        public void Save(string fileName, object obj)
        {
            throw new System.NotImplementedException();
        }

        public string SerializeObjectToStringRecursively(object obj)
        {
            string objectType = obj.GetType().FullName;
            throw new System.NotImplementedException();
        }

        public void DeserializeObjectFromString(string serializedString)
        {
            throw new System.NotImplementedException();
        }
        
        
        public static void SaveToBinary (string fileName, object obj)
		{
            FileStream file = new FileStream($"{fileName}", FileMode.Create);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(file, obj);
            file.Close();
        }
        public static T LoadFromBinary<T>(string fileName)
        {
            try
			{
                FileStream file = new FileStream($"{fileName}", FileMode.Open);
                BinaryFormatter formatter = new BinaryFormatter();
                T obj = (T)formatter.Deserialize(file);
                file.Close();
                return obj;
            }
            catch (SerializationException serializationException)
			{
                throw new SerializationException($"Failed to deserialize file {fileName} - {serializationException.Message}");
			}
        }
        public static void SaveToXml (string fileName, object obj)
		{
            string data = SerializeToXml(obj);
            StreamWriter writer = new StreamWriter($"{fileName}.xml");
            writer.Write(data);
            writer.Close();
		}
        public static T LoadFromXml <T> (string fileName)
		{
            StreamReader reader = new StreamReader($"{fileName}.xml");
            string data = reader.ReadToEnd();
            reader.Close();
            return DeserializeFromXml<T>(data);
		}
        static string SerializeToXml(object obj)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
                serializer.WriteObject(memoryStream, obj);
                memoryStream.Position = 0;
                return reader.ReadToEnd();
            }
        }
        static T DeserializeFromXml <T> (string xml)
        {
            using (Stream stream = new MemoryStream())
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(T));
                return (T)deserializer.ReadObject(stream);
            }
        }

    }
}
*/