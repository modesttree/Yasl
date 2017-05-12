using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace ModestTree.Util
{
    public class PrimitiveClassSerializer<T> : Serializer<T>
        where T : class
    {
        public override void SerializeConstructor(T value, ISerializationWriter writer)
        {
            writer.WritePrimitive<T>(value);
        }

        public override T DeserializeConstructor(int version, ISerializationReader reader)
        {
            return reader.ReadPrimitive<T>();
        }
    }
}

