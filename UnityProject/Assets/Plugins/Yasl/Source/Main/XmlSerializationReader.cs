using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.Serialization;

namespace ModestTree.Util
{
    public class XmlSerializationReader : ISerializationReader
    {
        readonly XDocument _doc;
        readonly Stack<XElement> _groups = new Stack<XElement>();
        readonly SortedDictionary<long, ReadInfo> _rootReferences = new SortedDictionary<long, ReadInfo>();
        readonly HashSet<ReadInfo> _activeConstructors = new HashSet<ReadInfo>();
        readonly HashSet<ReadInfo> _activeContents = new HashSet<ReadInfo>();
        int _version;

        public XmlSerializationReader(string xmlText)
        {
            _doc = XDocument.Parse(xmlText);
        }

        XElement CurrentGroup
        {
            get { return _groups.Peek(); }
        }

        public int Version
        {
            get
            {
                return (int)_doc.Root.GetAttribute("version");
            }
        }

        public object Read(Type knownType, string name, bool allowCircularDependencies)
        {
            Assert.IsNotNull(name);

            var groupElem = CurrentGroup.GetElement(name);
            _groups.Push(groupElem);
            var result = ReadContents(knownType, allowCircularDependencies);
            _groups.Pop();

            return result;
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
            foreach (var element in CurrentGroup.Elements(itemName))
            {
                _groups.Push(element);
                yield return ReadContents(knownType, allowCircularDependencies);
                _groups.Pop();
            }
        }

        object ReadValueContents(Type knownType)
        {
            Type type;
            var typeAttrib = CurrentGroup.Attribute("type");

            if (typeAttrib == null)
            {
                type = knownType;
            }
            else if (typeAttrib.Value == "null")
            {
                return null;
            }
            else
            {
                type = SerializerRegistry.ParseSerializedType(typeAttrib.Value);
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
            long id = (long)CurrentGroup.Attribute("ref");

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
                return bool.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(byte))
            {
                return byte.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(short))
            {
                return short.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(ushort))
            {
                return ushort.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(int))
            {
                return int.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(uint))
            {
                return uint.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(long))
            {
                return long.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(ulong))
            {
                return ulong.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(float))
            {
                return float.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(double))
            {
                return double.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(decimal))
            {
                return decimal.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(char))
            {
                return char.Parse(CurrentGroup.Value);
            }
            else if (knownType == typeof(string))
            {
                return CurrentGroup.Value;
            }
            else if (knownType == typeof(byte[]))
            {
                return Convert.FromBase64String(CurrentGroup.Value);
            }
            else if (knownType == typeof(Type))
            {
                return SerializerRegistry.ParseSerializedType(CurrentGroup.Value);
            }
            else if (knownType.IsEnum())
            {
                return Enum.Parse(knownType, CurrentGroup.Value);
            }

            throw Assert.CreateException(
                "unhandled primative type '{0}'", knownType.Name());
        }

        public T ReadXml<T>()
        {
            var result = ReadXml();
            Assert.IsType<T>(result, "Invalid root type found when deserializing xml");
            return (T)result;
        }

        public object ReadXml()
        {
            var root = _doc.Root;
            long rootId = (long)root.GetAttribute("id");
            _version = (int)root.GetAttribute("version");

            Assert.That(_rootReferences.IsEmpty());

            foreach (var element in root.Elements())
            {
                Assert.That(element.Name.LocalName == "Reference");

                var id = (long)element.GetAttribute("id");
                var typeName = (string)element.GetAttribute("type");

                var info = new ReadInfo();

                info.Element = element;
                info.SerializedType = SerializerRegistry.ParseSerializedType(typeName);
                info.Serializer = SerializerRegistry.GetSerializer(info.SerializedType);

                _rootReferences.Add(id, info);
            }

            foreach (var info in _rootReferences.Values)
            {
                DeserializeConstructor(info);
                DeserializeContents(info, false);
            }

            foreach (var info in _rootReferences.Values)
            {
                _groups.Push(info.Element);
                info.Serializer.DeserializeBacklinks(ref info.Value, _version, this);
                _groups.Pop();
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

            _groups.Push(info.Element);

            info.Serializer.DeserializeConstructor(out info.Value, _version, this);
            info.HasReadConstructor = true;

            _groups.Pop();

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

            _groups.Push(info.Element);

            info.Serializer.DeserializeContents(ref info.Value, _version, this);
            info.HasReadContents = true;

            _groups.Pop();

            _activeContents.Remove(info);
        }

        class ReadInfo
        {
            public XElement Element;
            public Type SerializedType;
            public ISerializer Serializer;
            public object Value;
            public bool HasReadConstructor;
            public bool HasReadContents;
        }
    }
}
