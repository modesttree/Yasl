using System;
using Yasl.Internal;

namespace Yasl
{
    public class InvalidSerializer : ISerializer
    {
        public InvalidSerializer()
        {
        }
        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("Used invalid serializer");
        }

        public void DeserializationComplete(ref object value, int version)
        {
            throw Assert.CreateException("Used invalid serializer");
        }
    }
}
