using System;
using System.Collections.Generic;

namespace Yasl
{
    public class KeyValuePairSerializer<TKey, TValue> : ISerializer
    {
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            var kvp = (KeyValuePair<TKey, TValue>)value;
            writer.Write(typeof(TKey), "Key", kvp.Key);
            writer.Write(typeof(TValue), "Value", kvp.Value);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            value = null;
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            TKey kvpKey = reader.Read<TKey>("Key");
            TValue kvpValue = reader.Read<TValue>("Value");
            value = new KeyValuePair<TKey, TValue>(kvpKey, kvpValue);
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
        }

        public void DeserializationComplete(ref object value, int version)
        {
        }
    }
}
