using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace Yasl
{
    public class Vector2Serializer : StructSerializer<Vector2>
    {
        public Vector2Serializer()
        {
        }

        public override void SerializeConstructor(ref Vector2 value, ISerializationWriter writer)
        {
            writer.Write<float>("X", value.x);
            writer.Write<float>("Y", value.y);
        }

        public override void DeserializeConstructor(out Vector2 value, int version, ISerializationReader reader)
        {
            value.x = reader.Read<float>("X");
            value.y = reader.Read<float>("Y");
        }
    }
}
