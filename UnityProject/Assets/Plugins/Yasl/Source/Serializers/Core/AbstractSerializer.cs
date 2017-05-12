using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using Yasl.Internal;

namespace Yasl
{
    public class AbstractSerializer<T> : ISerializer
        where T : class
    {
        public virtual void SerializeContents(T value, ISerializationWriter writer)
        {
        }

        public virtual void DeserializeContents(T value, int version, ISerializationReader reader)
        {
        }

        public virtual void SerializeBacklinks(T value, ISerializationWriter writer)
        {
        }

        public virtual void DeserializeBacklinks(T value, int version, ISerializationReader reader)
        {
        }

        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException(
                "can't serialize constructor of abstract type '{0}'", typeof(T).Name());
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException(
                "can't deserialize constructor of abstract type '{0}'", typeof(T).Name());
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeContents(typed, writer);
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeContents(typed, version, reader);
            value = typed;
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeBacklinks(typed, writer);
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeBacklinks(typed, version, reader);
            value = typed;
        }

        public virtual void DeserializationComplete(ref object value, int version)
        {
        }
    }
}
