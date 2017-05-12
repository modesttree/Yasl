using System;
using System.Linq;
using System.Collections.Generic;

namespace Yasl
{
    public class ListSerializer<T> : ISerializer
    {
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            writer.WriteSet<T>(SerializationConstants.DefaultValueItemName, (List<T>)value);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            value = new List<T>();
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            var list = (List<T>)value;

            var items = reader.ReadSet<T>(SerializationConstants.DefaultValueItemName);
            list.AddRange(items);
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
        }

        public void DeserializationComplete(ref object value, int version)
        {
        }
    }
}

