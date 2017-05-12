using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using System.Linq;

namespace ModestTree.Util
{
    public static class SerializationManager
    {
        public static XElement SerializeToXml(string rootName, object rootObject)
        {
            Assert.IsNotNull(rootObject);
            var writer = new XmlSerializationWriter();
            return writer.WriteXml(rootName, rootObject);
        }

        public static void SerializeToBinary(object rootObject, Stream stream)
        {
            var writer = new BinarySerializationWriter();
            writer.WriteToBinary(stream, rootObject);
        }

        public static byte[] SerializeToBinaryArray(object rootObject)
        {
            using (var stream = new MemoryStream())
            {
                SerializeToBinary(rootObject, stream);
                return stream.ToArray();
            }
        }

        public static T DeserializeFromXml<T>(string xmlStr)
        {
            return CreateXmlReader(xmlStr).ReadXml<T>();
        }

        // This is useful instead of DeserializeFromXml if you want
        // the version as well
        public static XmlSerializationReader CreateXmlReader(string xmlStr)
        {
            return new XmlSerializationReader(xmlStr);
        }

        // This is useful instead of DeserializeFromBinary if you want
        // the version as well
        public static BinarySerializationReader CreateBinaryReader(Stream stream)
        {
            return new BinarySerializationReader(stream);
        }

        public static object DeserializeFromXml(string xmlStr)
        {
            return CreateXmlReader(xmlStr).ReadXml();
        }

        public static T DeserializeFromBinary<T>(Stream stream)
        {
            return CreateBinaryReader(stream).ReadFromBinary<T>();
        }

        public static object DeserializeFromBinary(Stream stream)
        {
            return CreateBinaryReader(stream).ReadFromBinary();
        }
    }
}
