using System;

namespace Yasl
{
    public abstract class StructSerializer<T> : ISerializer
        where T : struct
    {
        public virtual void SerializeConstructor(ref T value, ISerializationWriter writer)
        {
        }

        public virtual void DeserializeConstructor(out T value, int version, ISerializationReader reader)
        {
            value = default(T);
        }

        public virtual void SerializeBacklinks(ref T value, ISerializationWriter writer)
        {
        }

        public virtual void SerializeContents(ref T value, ISerializationWriter writer)
        {
        }

        public virtual void DeserializeContents(ref T value, int version, ISerializationReader reader)
        {
        }

        public virtual void DeserializeBacklinks(ref T value, int version, ISerializationReader reader)
        {
        }

        public virtual void DeserializationComplete(ref T value, int version)
        {
        }

        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeConstructor(ref typed, writer);
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeContents(ref typed, writer);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
            T typed = (T)value;
            SerializeBacklinks(ref typed, writer);
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            T typed;
            DeserializeConstructor(out typed, version, reader);
            value = typed;
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeContents(ref typed, version, reader);
            value = typed;
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
            T typed = (T)value;
            DeserializeBacklinks(ref typed, version, reader);
            value = typed;
        }

        public void DeserializationComplete(ref object value, int version)
        {
            T typed = (T)value;
            DeserializationComplete(ref typed, version);
            value = typed;
        }
    }
}
