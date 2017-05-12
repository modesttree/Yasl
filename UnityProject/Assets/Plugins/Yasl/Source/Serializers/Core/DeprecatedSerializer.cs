using System;
using Yasl.Internal;

namespace Yasl
{
    public class DeprecatedSerializer : ISerializer
    {
        public DeprecatedSerializer()
        {
        }

        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }

        public void DeserializationComplete(ref object value, int version)
        {
            throw Assert.CreateException("This serializer was marked deprecated and should not be used anymore");
        }
    }
}
