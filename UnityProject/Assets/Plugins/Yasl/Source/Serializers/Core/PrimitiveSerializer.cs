using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace ModestTree.Util
{
    public class PrimitiveSerializer<T> : StructSerializer<T>
        where T : struct
    {
        public override void SerializeConstructor(ref T value, ISerializationWriter writer)
        {
            writer.WritePrimitive<T>(value);
        }

        public override void DeserializeConstructor(out T value, int version, ISerializationReader reader)
        {
            value = reader.ReadPrimitive<T>();
        }
    }
}
