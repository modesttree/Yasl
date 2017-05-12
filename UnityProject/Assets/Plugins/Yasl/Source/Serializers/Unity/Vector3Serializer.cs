using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace Yasl
{
    public class Vector3Serializer : StructSerializer<Vector3>
    {
        public Vector3Serializer()
        {
        }

        public override void SerializeConstructor(ref Vector3 value, ISerializationWriter writer)
        {
            writer.Write<float>("X", value.x);
            writer.Write<float>("Y", value.y);
            writer.Write<float>("Z", value.z);
        }

        public override void DeserializeConstructor(out Vector3 value, int version, ISerializationReader reader)
        {
            value.x = reader.Read<float>("X");
            value.y = reader.Read<float>("Y");
            value.z = reader.Read<float>("Z");
        }
    }
}
