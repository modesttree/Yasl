using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.IO;
using Yasl.Internal;

namespace Yasl
{
    public class BinarySerializationWriter : ISerializationWriter
    {
        readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        readonly SortedList<long, object> _references = new SortedList<long, object>();
        readonly Queue<long> _pending = new Queue<long>();

        BinaryWriter _currentPass;

        public void Write(Type knownType, string groupName, object value)
        {
            WriteContents(knownType, value);
        }

        void WriteContents(Type knownType, object value)
        {
            if (SerializerRegistry.IsPrimitive(knownType))
            {
                WriteValueContents(knownType, value);
            }
            else
            {
                WriteReferenceContents(knownType, value);
            }
        }

        public void IncludeReference(object value)
        {
            if (value == null)
            {
                return;
            }

            bool firstTime;
            long id = _objectIdGenerator.GetId(value, out firstTime);
            if (!_references.ContainsKey(id))
            {
                _references.Add(id, value);
                _pending.Enqueue(id);
            }
        }

        void WriteReferenceContents(Type knownType, object value)
        {
            if (value == null)
            {
                _currentPass.Write((long)0);
            }
            else
            {
                IncludeReference(value);

                bool firstTime;
                long id = _objectIdGenerator.GetId(value, out firstTime);
                Assert.That(_references.ContainsKey(id), "referenced object not included");
                _currentPass.Write(id);
            }
        }

        void WriteValueContents(Type knownType, object value)
        {
            if (value == null)
            {
                _currentPass.Write((int)-1);
            }
            else
            {
                Type type = value.GetType();

                if (type.DerivesFrom<Type>())
                {
                    // Treat RuntimeType as if it's Type
                    type = typeof(Type);
                }

                if (knownType == type)
                {
                    _currentPass.Write((int)-2);
                }
                else
                {
                    SerializerRegistry.WriteSerializedTypeId(type, _currentPass);
                }

                var serializer = SerializerRegistry.GetSerializer(type);
                serializer.SerializeConstructor(ref value, this);
                serializer.SerializeContents(ref value, this);
                serializer.SerializeBacklinks(ref value, this);
            }
        }

        public void WriteSet(Type knownType, string itemName, IEnumerable set)
        {
            Assert.IsNotNull(set);

            object[] setArray = set.Cast<object>().ToArray();

            _currentPass.Write(setArray.Length);

            for (int i = 0; i < setArray.Length; ++i)
            {
                Write(knownType, itemName, setArray[i]);
            }
        }

        public void WritePrimitive(Type knownType, object value)
        {
            Assert.IsNotNull(knownType);
            Assert.IsNotNull(value);

            if (knownType == typeof(bool))
            {
                _currentPass.Write((bool)value);
            }
            else if (knownType == typeof(byte))
            {
                _currentPass.Write((byte)value);
            }
            else if (knownType == typeof(short))
            {
                _currentPass.Write((short)value);
            }
            else if (knownType == typeof(ushort))
            {
                _currentPass.Write((ushort)value);
            }
            else if (knownType == typeof(int))
            {
                _currentPass.Write((int)value);
            }
            else if (knownType == typeof(uint))
            {
                _currentPass.Write((uint)value);
            }
            else if (knownType == typeof(long))
            {
                _currentPass.Write((long)value);
            }
            else if (knownType == typeof(ulong))
            {
                _currentPass.Write((ulong)value);
            }
            else if (knownType == typeof(float))
            {
                _currentPass.Write((float)value);
            }
            else if (knownType == typeof(double))
            {
                _currentPass.Write((double)value);
            }
            else if (knownType == typeof(decimal))
            {
                _currentPass.Write((decimal)value);
            }
            else if (knownType == typeof(char))
            {
                _currentPass.Write((char)value);
            }
            else if (knownType == typeof(string))
            {
                _currentPass.Write((string)value);
            }
            else if (knownType == typeof(byte[]))
            {
                var bytes = (byte[])value;
                _currentPass.Write((int)bytes.Length);
                _currentPass.Write(bytes);
            }
            else if (knownType == typeof(Type))
            {
                SerializerRegistry.WriteSerializedTypeId((Type)value, _currentPass);
            }
            else if (knownType.IsEnum())
            {
                WritePrimitive(Enum.GetUnderlyingType(knownType), value);
            }
            else
            {
                throw Assert.CreateException(
                    "unhandled primative type '{0}'", knownType.Name());
            }
        }

        public void WriteToBinary(Stream stream, object rootReference)
        {
            using (MemoryStream constructorBuffer = new MemoryStream())
            using (MemoryStream contentsBuffer = new MemoryStream())
            using (MemoryStream backlinkBuffer = new MemoryStream())
            {
                BinaryWriter constructorWriter = new BinaryWriter(constructorBuffer);
                BinaryWriter contentsWriter = new BinaryWriter(contentsBuffer);
                BinaryWriter backlinkWriter = new BinaryWriter(backlinkBuffer);

                BinaryWriter headerWriter = new BinaryWriter(stream);
                headerWriter.Write(SerializationVersion.Value);

                IncludeReference(rootReference);

                bool firstTime;
                long rootId = _objectIdGenerator.GetId(rootReference, out firstTime);
                Assert.That(_references.ContainsKey(rootId), "root object not included");

                while (_pending.Count > 0)
                {
                    long id = _pending.Dequeue();
                    Assert.That(id != 0);

                    object value = _references[id];
                    Type type = value.GetType();
                    var serializer = SerializerRegistry.GetSerializer(type);

                    headerWriter.Write(id);
                    headerWriter.Write(constructorWriter.BaseStream.Position);
                    headerWriter.Write(contentsWriter.BaseStream.Position);
                    headerWriter.Write(backlinkWriter.BaseStream.Position);
                    SerializerRegistry.WriteSerializedTypeId(type, headerWriter);

                    try
                    {
                        _currentPass = constructorWriter;
                        serializer.SerializeConstructor(ref value, this);

                        _currentPass = contentsWriter;
                        serializer.SerializeContents(ref value, this);

                        _currentPass = backlinkWriter;
                        serializer.SerializeBacklinks(ref value, this);

                        _currentPass = null;
                    }
                    catch (Exception e)
                    {
                        throw new SerializationException(
                            "Error occurred while serializing value of type '" + type.Name() + "'", e);
                    }
                }

                headerWriter.Write((long)0);
                headerWriter.Write(rootId);

                constructorWriter.Flush();
                contentsWriter.Flush();
                backlinkWriter.Flush();

                long headerOffsetsLength = 8 * 4;
                long constructorsOffset = headerWriter.BaseStream.Position + headerOffsetsLength;
                long contentsOffset = constructorsOffset + constructorBuffer.Length;
                long backlinkOffset = contentsOffset + contentsBuffer.Length;
                long endOffset = contentsOffset + contentsBuffer.Length;

                headerWriter.Write(constructorsOffset);
                headerWriter.Write(contentsOffset);
                headerWriter.Write(backlinkOffset);
                headerWriter.Write(endOffset);

                constructorBuffer.WriteTo(stream);
                contentsBuffer.WriteTo(stream);
                backlinkBuffer.WriteTo(stream);
            }
        }
    }
}
