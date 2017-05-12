using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Yasl
{
    public interface ISerializer
    {
        void SerializeConstructor(ref object value, ISerializationWriter writer);
        void SerializeContents(ref object value, ISerializationWriter writer);
        void SerializeBacklinks(ref object value, ISerializationWriter writer);

        void DeserializeConstructor(out object value, int version, ISerializationReader reader);
        void DeserializeContents(ref object value, int version, ISerializationReader reader);
        void DeserializeBacklinks(ref object value, int version, ISerializationReader reader);

        void DeserializationComplete(ref object value, int version);
    }

    public class SerializationConstants
    {
        public const string DefaultValueItemName = "Item";
    }

    public interface ISerializationWriter
    {
        void Write(Type knownType, string groupName, object value);
        void WriteSet(Type knownType, string itemName, IEnumerable set);
        void WritePrimitive(Type knownType, object value);
    }

    public interface ISerializationReader
    {
        object Read(Type knownType, string name, bool allowCircularDependencies);
        IEnumerable<object> ReadSet(Type knownType, string itemName, bool allowCircularDependencies);
        object ReadPrimitive(Type knownType);
    }
}
