using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ModestTree.Util
{
    public class XmlSerializationHelper
    {
        static XmlWriterSettings _settings;

        static XmlSerializationHelper()
        {
            _settings = new XmlWriterSettings();
            _settings.Encoding = System.Text.Encoding.UTF8;
            _settings.CloseOutput = true;
            _settings.Indent = true;
        }

        public static void SerializeToFile<T>(T obj, string filePath)
        {
            var xml = new XmlSerializer(typeof(T));

            using (var streamWriter = File.CreateText(filePath))
            using (var xmlWriter = XmlWriter.Create(streamWriter, _settings))
            {
                xml.Serialize(xmlWriter, obj);
                xmlWriter.Flush();
            }
        }

        public static T DeserializeFromFile<T>(string filePath)
            where T : class
        {
            var xml = new XmlSerializer(typeof(T));

            using (var fileStream = File.OpenRead(filePath))
            {
                return xml.Deserialize(fileStream) as T;
            }
        }

        public static string SerializeToText<T>(T obj) where T : class
        {
            var xml = new XmlSerializer(typeof(T));

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter, _settings))
            {
                xml.Serialize(xmlWriter, obj);
                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }

        public static T DeserializeFromText<T>(string text) where T : class
        {
            var xml = new XmlSerializer(typeof(T));

            var stringReader = new StringReader(text);
            return xml.Deserialize(stringReader) as T;
        }

        public static byte[] SerializeToBinary<T>(T obj) where T : class
        {
            var xml = new XmlSerializer(typeof(T));

            using (var memoryWriter = new MemoryStream())
            using (var xmlWriter = XmlWriter.Create(memoryWriter, _settings))
            {
                xml.Serialize(xmlWriter, obj);
                xmlWriter.Flush();
                return memoryWriter.ToArray();
            }
        }

        public static T DeserializeFromBinary<T>(byte[] binary) where T : class
        {
            var xml = new XmlSerializer(typeof(T));

            var memoryReader = new MemoryStream(binary);
            return xml.Deserialize(memoryReader) as T;
        }
    }
}
