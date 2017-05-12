using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Yasl
{
    public class TimeSpanSerializer : StructSerializer<TimeSpan>
    {
        public TimeSpanSerializer()
        {
        }

        public override void SerializeConstructor(ref TimeSpan value, ISerializationWriter writer)
        {
            writer.WritePrimitive<long>(value.Ticks);
        }

        public override void DeserializeConstructor(out TimeSpan value, int version, ISerializationReader reader)
        {
            long ticks = reader.ReadPrimitive<long>();
            value = new TimeSpan(ticks);
        }
    }
}
