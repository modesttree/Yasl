using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;
using System.Runtime.CompilerServices;
using Yasl.Internal;

namespace Yasl
{
    public class XmlSerializationWriter : ISerializationWriter
    {
        readonly ObjectIDGenerator _objectIdGenerator = new ObjectIDGenerator();

        readonly Stack<XElement> _groups = new Stack<XElement>();
        readonly SortedList<long, object> _references = new SortedList<long, object>();
        readonly SortedList<long, XElement> _lookup = new SortedList<long, XElement>();
        readonly Queue<long> _pending = new Queue<long>();

        bool _hasWritten;

        XElement CurrentGroup
        {
            get { return _groups.Peek(); }
        }

        public void IncludeReference(object value)
        {
            if (value == null)
            {
                return;
            }

            Type type = value.GetType();

            bool firstTime;
            long id = _objectIdGenerator.GetId(value, out firstTime);
            if (!_lookup.ContainsKey(id))
            {
                XElement rootElement = new XElement("Reference");
                rootElement.Add(new XAttribute("id", id));
                rootElement.Add(new XAttribute("type", SerializerRegistry.GetSerializedTypeName(type)));
                _lookup.Add(id, rootElement);
                _references.Add(id, value);
                _pending.Enqueue(id);
            }
        }

        public void WriteSet(Type knownType, string itemName, IEnumerable set)
        {
            Assert.IsNotNull(itemName);
            Assert.IsNotNull(set, "Sets cannot be null because we assume empty tag value = empty collection during read");

            foreach (var item in set)
            {
                Write(knownType, itemName, item);
            }
        }

        public void Write(Type knownType, string groupName, object value)
        {
            Assert.IsNotNull(groupName);

            var element = new XElement(groupName);
            CurrentGroup.Add(element);
            _groups.Push(element);

            if (SerializerRegistry.IsPrimitive(knownType))
            {
                WriteValueContents(knownType, value);
            }
            else
            {
                WriteReferenceContents(knownType, value);
            }

            _groups.Pop();
        }

        void WriteReferenceContents(Type knownType, object value)
        {
            if (value == null)
            {
                CurrentGroup.Add(new XAttribute("ref", "0"));
            }
            else
            {
                IncludeReference(value);

                bool firstTime;
                long id = _objectIdGenerator.GetId(value, out firstTime);
                Assert.That(_lookup.ContainsKey(id), "referenced object not included");

                CurrentGroup.Add(new XAttribute("ref", id));
            }
        }

        void WriteValueContents(Type knownType, object value)
        {
            if (value == null)
            {
                CurrentGroup.Add(new XAttribute("type", "null"));
            }
            else
            {
                Type type = value.GetType();

                if (type.DerivesFrom<Type>())
                {
                    // Treat RuntimeType as if it's Type
                    type = typeof(Type);
                }

                var serializer = SerializerRegistry.GetSerializer(type);
                serializer.SerializeConstructor(ref value, this);
                serializer.SerializeContents(ref value, this);
                serializer.SerializeBacklinks(ref value, this);

                if (type != knownType)
                {
                    CurrentGroup.Add(new XAttribute("type", SerializerRegistry.GetSerializedTypeName(type)));
                }
            }
        }

        public void WritePrimitive(Type knownType, object value)
        {
            Assert.IsNotNull(knownType);
            Assert.IsNotNull(value);

            if (knownType == typeof(bool))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(byte))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(short))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(ushort))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(int))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(uint))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(long))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(ulong))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(float))
            {
                CurrentGroup.Value = ((float)value).ToString("R");
            }
            else if (knownType == typeof(double))
            {
                CurrentGroup.Value = ((double)value).ToString("R");
            }
            else if (knownType == typeof(decimal))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(char))
            {
                CurrentGroup.Value = value.ToString();
            }
            else if (knownType == typeof(string))
            {
                CurrentGroup.Value = (string)value;
            }
            else if (knownType == typeof(byte[]))
            {
                CurrentGroup.Value = Convert.ToBase64String((byte[])value);
            }
            else if (knownType == typeof(Type))
            {
                CurrentGroup.Value = SerializerRegistry.GetSerializedTypeName((Type)value);
            }
            else if (knownType.IsEnum())
            {
                CurrentGroup.Value = value.ToString();
            }
            else
            {
                throw Assert.CreateException(
                    "unhandled primative type '{0}'", knownType.Name());
            }
        }

        public XElement WriteXml(string rootName, object rootReference)
        {
            Assert.That(!_hasWritten);
            _hasWritten = true;

            IncludeReference(rootReference);

            Assert.That(_groups.Count == 0);

            bool firstTime;
            long rootId = _objectIdGenerator.GetId(rootReference, out firstTime);
            Assert.That(_references.ContainsKey(rootId), "root object not included");

            while(_pending.Count > 0)
            {
                long id = _pending.Dequeue();
                object value = _references[id];
                Type type = value.GetType();
                var serializer = SerializerRegistry.GetSerializer(type);

                try
                {
                    _groups.Push(_lookup[id]);
                    serializer.SerializeConstructor(ref value, this);
                    serializer.SerializeContents(ref value, this);
                    serializer.SerializeBacklinks(ref value, this);
                    _groups.Pop();
                }
                catch (Exception e)
                {
                    throw new SerializationException(
                        "Error occurred while serializing value of type '" + type.Name() + "'", e);
                }
            }

            Assert.That(_groups.Count == 0);

            XElement rootElement = new XElement(rootName);
            rootElement.Add(new XAttribute("id", rootId));
            rootElement.Add(new XAttribute("version", SerializationVersion.Value));
            foreach (var cur in _lookup.Values)
            {
                rootElement.Add(cur);
            }
            return rootElement;
        }
    }
}
