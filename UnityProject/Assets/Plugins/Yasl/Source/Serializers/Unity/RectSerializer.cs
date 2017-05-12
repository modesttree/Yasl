using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using UnityEngine;

namespace Yasl
{
    public class RectSerializer : StructSerializer<Rect>
    {
        public RectSerializer()
        {
        }

        public override void SerializeConstructor(ref Rect value, ISerializationWriter writer)
        {            
            writer.Write<float>("X", value.x);
            writer.Write<float>("Y", value.y);
            writer.Write<float>("Width", value.width);
            writer.Write<float>("Height", value.height);
        }

        public override void DeserializeConstructor(out Rect value, int version, ISerializationReader reader)
        {
            value = new Rect();                       
            value.x = reader.Read<float>("X");
            value.y = reader.Read<float>("Y");
            value.width = reader.Read<float>("Width");
            value.height = reader.Read<float>("Height");
        }
    }
}
