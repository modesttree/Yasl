using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace Yasl
{
    public class LinkedListSerializer<T> : ISerializer
    {
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            writer.WriteSet<T>(SerializationConstants.DefaultValueItemName, (LinkedList<T>)value);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            value = new LinkedList<T>();
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            LinkedList<T> list = (LinkedList<T>)value;

            var items = reader.ReadSet<T>(SerializationConstants.DefaultValueItemName);

            foreach (var cur in items)
            {
                list.AddLast((T)cur);
            }
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
        }

        public void DeserializationComplete(ref object value, int version)
        {
        }
    }
}
