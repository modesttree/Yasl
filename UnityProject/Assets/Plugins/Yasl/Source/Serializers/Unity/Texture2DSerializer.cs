using System;
using Yasl;
using UnityEngine;
using Yasl.Internal;

namespace Yasl
{
    public class Texture2DSerializer : Serializer<Texture2D>
    {
        public Texture2DSerializer()
        {
        }

        public override void SerializeConstructor(Texture2D value, ISerializationWriter writer)
        {
            writer.Write<int>("Width", value.width);
            writer.Write<int>("Height", value.height);
            writer.Write<TextureFormat>("Format", value.format);
        }

        public override void SerializeContents(Texture2D value, ISerializationWriter writer)
        {
            writer.Write<byte[]>("PngData", value.EncodeToPNG());
        }

        public override Texture2D DeserializeConstructor(int version, ISerializationReader reader)
        {
            var width = reader.Read<int>("Width");
            var height = reader.Read<int>("Height");
            var format = reader.Read<TextureFormat>("Format");
            return new Texture2D(width, height, format, false);
        }

        public override void DeserializeContents(Texture2D value, int version, ISerializationReader reader)
        {
            var data = reader.Read<byte[]>("PngData");
            var success = value.LoadImage(data);
            Assert.That(success);
        }
    }
}
