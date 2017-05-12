using System;
using Yasl.Internal;

namespace Yasl
{
    public abstract class Serializer<T> : ISerializer
        where T : class
    {
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            Assert.IsType<T>(value);
            T typed = (T)value;
            SerializeConstructor(typed, writer);
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeContents(typed, writer);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeBacklinks(typed, writer);
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            value = DeserializeConstructor(version, reader);
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeContents(typed, version, reader);
            value = typed;
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeBacklinks(typed, version, reader);
            value = typed;
        }

        public void DeserializationComplete(ref object value, int version)
        {
            T typed = (T)value;
            DeserializationComplete(typed, version);
        }

        public virtual void SerializeConstructor(T value, ISerializationWriter writer)
        {
        }

        public virtual void SerializeContents(T value, ISerializationWriter writer)
        {
        }

        public virtual void SerializeBacklinks(T value, ISerializationWriter writer)
        {
        }

        public abstract T DeserializeConstructor(int version, ISerializationReader reader);

        public virtual void DeserializeContents(T value, int version, ISerializationReader reader)
        {
        }

        public virtual void DeserializeBacklinks(T value, int version, ISerializationReader reader)
        {
        }

        public virtual void DeserializationComplete(T value, int version)
        {
        }
    }

    // Convenience class for types with default constructor
    public abstract class SerializerSimple<T> : Serializer<T>
        where T : class, new()
    {
        public override T DeserializeConstructor(int version, ISerializationReader reader)
        {
            return new T();
        }
    }
}
