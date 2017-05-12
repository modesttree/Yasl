using ModestTree.Util;
using UnityEngine;

namespace ModestTree.Modest3d
{
    public class KeyframeSerializer : StructSerializer<Keyframe>
    {
        public KeyframeSerializer()
        {
        }

        public override void SerializeConstructor(ref Keyframe value, ISerializationWriter writer)
        {
            writer.Write<float>("Time", value.time);
            writer.Write<float>("Value", value.value);
            writer.Write<float>("InTangent", value.inTangent);
            writer.Write<float>("OutTangent", value.outTangent);
        }

        public override void DeserializeConstructor(out Keyframe keyFrame, int version, ISerializationReader reader)
        {
            var time = reader.Read<float>("Time");
            var value = reader.Read<float>("Value");
            var inTangent = reader.Read<float>("InTangent");
            var outTangent = reader.Read<float>("OutTangent");

            keyFrame = new Keyframe(time, value, inTangent, outTangent);
        }
    }
}
