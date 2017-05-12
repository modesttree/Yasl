using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace ModestTree.Util
{
    public class DateTimeSerializer : StructSerializer<DateTime>
    {
        public DateTimeSerializer()
        {
        }

        public override void SerializeConstructor(ref DateTime value, ISerializationWriter writer)
        {
            writer.WritePrimitive<long>(value.Ticks);
        }

        public override void DeserializeConstructor(out DateTime value, int version, ISerializationReader reader)
        {
            long ticks = reader.ReadPrimitive<long>();
            value = new DateTime(ticks);
        }
    }
}
