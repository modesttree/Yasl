using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace Yasl
{
    public class RectOffsetSerializer : Serializer<RectOffset>
    {
        public RectOffsetSerializer()
        {
        }

        public override void SerializeConstructor(RectOffset value, ISerializationWriter writer)
        {
            writer.Write<int>("Left", value.left);
            writer.Write<int>("Right", value.right);
            writer.Write<int>("Bottom", value.bottom);
            writer.Write<int>("Top", value.top);
        }

        public override RectOffset DeserializeConstructor(int version, ISerializationReader reader)
        {
            RectOffset value = new RectOffset();
            value.left = reader.Read<int>("Left");
            value.right = reader.Read<int>("Right");
            value.bottom = reader.Read<int>("Bottom");
            value.top = reader.Read<int>("Top");
            return value;
        }
    }
}
