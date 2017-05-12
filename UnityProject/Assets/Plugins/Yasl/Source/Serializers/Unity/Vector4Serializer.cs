using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace ModestTree.Util
{
    public class Vector4Serializer : StructSerializer<Vector4>
    {
        public Vector4Serializer()
        {
        }

        public override void SerializeConstructor(ref Vector4 value, ISerializationWriter writer)
        {
            writer.Write<float>("W", value.w);
            writer.Write<float>("X", value.x);
            writer.Write<float>("Y", value.y);
            writer.Write<float>("Z", value.z);
        }

        public override void DeserializeConstructor(out Vector4 value, int version, ISerializationReader reader)
        {
            value.w = reader.Read<float>("W");
            value.x = reader.Read<float>("X");
            value.y = reader.Read<float>("Y");
            value.z = reader.Read<float>("Z");
        }
    }
}
