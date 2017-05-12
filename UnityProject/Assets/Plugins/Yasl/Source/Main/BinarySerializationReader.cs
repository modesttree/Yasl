using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;

namespace ModestTree.Util
{
    public class BinarySerializationReader : ISerializationReader
    {
        readonly BinaryReader _binaryReader;
        readonly Dictionary<long, ReadInfo> _rootReferences = new Dictionary<long, ReadInfo>();
        readonly HashSet<ReadInfo> _activeConstructors = new HashSet<ReadInfo>();
        readonly HashSet<ReadInfo> _activeContents = new HashSet<ReadInfo>();

        int _version;

        public BinarySerializationReader(Stream stream)
        {
            _binaryReader = new BinaryReader(stream);
        }

        public object Read(Type knownType, string name, bool allowCircularDependencies)
        {
            // no group heading for binary
            return ReadContents(knownType, allowCircularDependencies);
        }

        object ReadContents(Type knownType, bool allowCircularDependencies)
        {
            if (SerializerRegistry.IsPrimitive(knownType))
            {
                return ReadValueContents(knownType);
            }
            else
            {
                return ReadReferenceContents(knownType, allowCircularDependencies);
            }
        }

        public IEnumerable<object> ReadSet(Type knownType, string itemName, bool allowCircularDependencies)
        {
            int count = _binaryReader.ReadInt32();

            for (int i = 0; i < count; ++i)
            {
                yield return ReadContents(knownType, allowCircularDependencies);
            }
        }

        object ReadValueContents(Type knownType)
        {
            int typeIndex = _binaryReader.ReadInt32();

            if (typeIndex == -1)
            {
                return null;
            }

            Type type;
            if (typeIndex == -2)
            {
                type = knownType;
            }
            else
            {
                type = SerializerRegistry.ReadSerializedTypeId(typeIndex, _binaryReader);
            }

            var serializer = SerializerRegistry.GetSerializer(type);

            object value;
            serializer.DeserializeConstructor(out value, _version, this);
            serializer.DeserializeContents(ref value, _version, this);
            serializer.DeserializeBacklinks(ref value, _version, this);
            return value;
        }

        object ReadReferenceContents(Type knownType, bool allowCircularDependencies)
        {
            long id = _binaryReader.ReadInt64();

            if (id == 0)
            {
                return null;
            }

            var info = _rootReferences[id];
            DeserializeConstructor(info);
            DeserializeContents(info, allowCircularDependencies);

            return info.Value;
        }

        public object ReadPrimitive(Type knownType)
        {
            Assert.IsNotNull(knownType);

            if (knownType == typeof(bool))
            {
                return _binaryReader.ReadBoolean();
            }
            else if (knownType == typeof(byte))
            {
                return _binaryReader.ReadByte();
            }
            else if (knownType == typeof(short))
            {
                return _binaryReader.ReadInt16();
            }
            else if (knownType == typeof(ushort))
            {
                return _binaryReader.ReadUInt16();
            }
            else if (knownType == typeof(int))
            {
                return _binaryReader.ReadInt32();
            }
            else if (knownType == typeof(uint))
            {
                return _binaryReader.ReadUInt32();
            }
            else if (knownType == typeof(long))
            {
                return _binaryReader.ReadInt64();
            }
            else if (knownType == typeof(ulong))
            {
                return _binaryReader.ReadUInt64();
            }
            else if (knownType == typeof(float))
            {
                return _binaryReader.ReadSingle();
            }
            else if (knownType == typeof(double))
            {
                return _binaryReader.ReadDouble();
            }
            else if (knownType == typeof(decimal))
            {
                return _binaryReader.ReadDecimal();
            }
            else if (knownType == typeof(char))
            {
                return _binaryReader.ReadChar();
            }
            else if (knownType == typeof(string))
            {
                return _binaryReader.ReadString();
            }
            else if (knownType == typeof(byte[]))
            {
                var length = _binaryReader.ReadInt32();
                return _binaryReader.ReadBytes(length);
            }
            else if (knownType == typeof(Type))
            {
                return SerializerRegistry.ReadSerializedTypeId(_binaryReader);
            }
            else if (knownType.IsEnum())
            {
                return Enum.ToObject(knownType, ReadPrimitive(Enum.GetUnderlyingType(knownType)));
            }

            throw Assert.CreateException(
                "unhandled primative type '{0}'", knownType.Name());
        }

        public T ReadFromBinary<T>()
        {
            var result = ReadFromBinary();
            Assert.IsType<T>(result, "Invalid root type found when deserializing binary data");
            return (T)result;
        }

        public object ReadFromBinary()
        {
            _version = _binaryReader.ReadInt32();

            List<ReadInfo> rootReferencesInOrder = new List<ReadInfo>();

            while (true)
            {
                long id = _binaryReader.ReadInt64();
                if (id == 0)
                {
                    break;
                }

                var info = new ReadInfo();
                info.Id = id;
                info.ConstructorPosition = _binaryReader.ReadInt64();
                info.ContentsPosition = _binaryReader.ReadInt64();
                info.BacklinkPosition = _binaryReader.ReadInt64();
                info.SerializedType = SerializerRegistry.ReadSerializedTypeId(_binaryReader);
                info.Serializer = SerializerRegistry.GetSerializer(info.SerializedType);

                _rootReferences.Add(id, info);
                rootReferencesInOrder.Add(info);
            }

            long rootId = _binaryReader.ReadInt64();
            long constructorSeekOffset = _binaryReader.ReadInt64();
            long contentsSeekOffset = _binaryReader.ReadInt64();
            long backlinkSeekOffset = _binaryReader.ReadInt64();

            // endSeekOffset
            _binaryReader.ReadInt64();

            foreach (var info in rootReferencesInOrder)
            {
                info.ConstructorPosition += constructorSeekOffset;
                info.ContentsPosition += contentsSeekOffset;
                info.BacklinkPosition += backlinkSeekOffset;
            }

            foreach (var info in rootReferencesInOrder)
            {
                DeserializeConstructor(info);
                DeserializeContents(info, false);
            }

            _binaryReader.BaseStream.Seek(backlinkSeekOffset, SeekOrigin.Begin);

            foreach (var info in rootReferencesInOrder)
            {
                info.Serializer.DeserializeBacklinks(ref info.Value, _version, this);
            }

            foreach (var info in _rootReferences.Values)
            {
                info.Serializer.DeserializationComplete(ref info.Value, _version);
            }

            return _rootReferences[rootId].Value;
        }

        void DeserializeConstructor(ReadInfo info)
        {
            if (info.HasReadConstructor)
            {
                return;
            }

            Assert.That(_activeConstructors.Add(info),
                "circular reference while reading constructor for '{0}'",
                info.SerializedType.Name());

            long returnPosition = _binaryReader.BaseStream.Position;
            _binaryReader.BaseStream.Seek(info.ConstructorPosition, SeekOrigin.Begin);

            info.Serializer.DeserializeConstructor(out info.Value, _version, this);
            info.HasReadConstructor = true;

            _binaryReader.BaseStream.Seek(returnPosition, SeekOrigin.Begin);

            _activeConstructors.Remove(info);
        }

        void DeserializeContents(ReadInfo info, bool allowCircularDependencies)
        {
            if (info.HasReadContents)
            {
                return;
            }

            Assert.That(info.HasReadConstructor);

            if (!_activeContents.Add(info))
            {
                if (!allowCircularDependencies)
                {
                    Log.Warn("Circular reference while reading contents for '{0}'", info.SerializedType.Name());
                }

                return;
            }

            long returnPosition = _binaryReader.BaseStream.Position;
            _binaryReader.BaseStream.Seek(info.ContentsPosition, SeekOrigin.Begin);

            info.Serializer.DeserializeContents(ref info.Value, _version, this);
            info.HasReadContents = true;

            _binaryReader.BaseStream.Seek(returnPosition, SeekOrigin.Begin);

            _activeContents.Remove(info);
        }

        class ReadInfo
        {
            public long Id;
            public long ConstructorPosition;
            public long ContentsPosition;
            public long BacklinkPosition;
            public Type SerializedType;
            public ISerializer Serializer;
            public object Value;
            public bool HasReadConstructor;
            public bool HasReadContents;
        }
    }
}
