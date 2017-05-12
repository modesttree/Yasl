using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace ModestTree.Util
{
    public class ColorSerializer : StructSerializer<Color>
    {
        public ColorSerializer()
        {
        }
        public override void SerializeConstructor(ref Color value, ISerializationWriter writer)
        {
            writer.Write<float>("R", value.r);
            writer.Write<float>("G", value.g);
            writer.Write<float>("B", value.b);
            writer.Write<float>("A", value.a);
        }

        public override void DeserializeConstructor(out Color value, int version, ISerializationReader reader)
        {
            value.r = reader.Read<float>("R");
            value.g = reader.Read<float>("G");
            value.b = reader.Read<float>("B");
            value.a = reader.Read<float>("A");
        }
    }
}
