using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace Yasl
{
    public class QuaternionSerializer : StructSerializer<Quaternion>
    {
        public QuaternionSerializer()
        {
        }

        public override void SerializeConstructor(ref Quaternion value, ISerializationWriter writer)
        {
            writer.Write<float>("W", value.w);
            writer.Write<float>("X", value.x);
            writer.Write<float>("Y", value.y);
            writer.Write<float>("Z", value.z);
        }

        public override void DeserializeConstructor(out Quaternion value, int version, ISerializationReader reader)
        {
            value.w = reader.Read<float>("W");
            value.x = reader.Read<float>("X");
            value.y = reader.Read<float>("Y");
            value.z = reader.Read<float>("Z");
        }
    }
}
