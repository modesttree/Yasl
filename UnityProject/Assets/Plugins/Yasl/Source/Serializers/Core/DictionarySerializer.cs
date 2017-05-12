using System;
using System.Collections.Generic;
using Yasl.Internal;

namespace Yasl
{
    public class DictionarySerializer<TKey, TValue> : ISerializer
    {
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            var dictionary = (Dictionary<TKey, TValue>)value;
            Assert.That(dictionary.Comparer == EqualityComparer<TKey>.Default, "only dictionaries using the default comparer may be serialized");
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            writer.WriteSet<KeyValuePair<TKey, TValue>>("Item", (Dictionary<TKey, TValue>)value);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            value = new Dictionary<TKey, TValue>();
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            var dictionary = (Dictionary<TKey, TValue>)value;
            foreach (var kvp in reader.ReadSet<KeyValuePair<TKey, TValue>>("Item"))
            {
                dictionary.Add(kvp.Key, kvp.Value);
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
