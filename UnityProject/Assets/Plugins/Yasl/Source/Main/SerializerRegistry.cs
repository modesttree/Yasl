using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Yasl.Internal;

namespace Yasl
{
    public static class SerializerRegistry
    {
        static readonly string _arrayTextId = "array";
        static readonly int _arrayBinaryId = -1;

        static readonly char[] _nameSplitCharacters = new char[] { ' ' };

        readonly static Dictionary<string, Definition> _namedDefinitions = new Dictionary<string, Definition>();
        readonly static Dictionary<Type, Definition> _typedDefinitions = new Dictionary<Type, Definition>();
        readonly static Dictionary<int, Definition> _indexToDefinition = new Dictionary<int, Definition>();
        readonly static Dictionary<Definition, int> _definitionToIndex = new Dictionary<Definition, int>();
        readonly static Dictionary<Type, ISerializer> _serializerInstances = new Dictionary<Type, ISerializer>();
        readonly static List<Definition> _definitions = new List<Definition>();

        static SerializerRegistry()
        {
            AddCoreSerializers();
            AddUnitySerializers();
        }

        static void AddUnitySerializers()
        {
            Add<RectOffset, RectOffsetSerializer>("RectOffset", 715620540);
            Add<Vector2, Vector2Serializer>("Vector2", 910695420);
            Add<Vector3, Vector3Serializer>("Vector3", 540378706);
            Add<Vector4, Vector4Serializer>("Vector4", 768841671);
            Add<Quaternion, QuaternionSerializer>("Quaternion", 923255225);
            Add<Color, ColorSerializer>("Color", 140544226);
            Add<Rect, RectSerializer>("Rect", 454130314);
        }

        static void AddCoreSerializers()
        {
            Add<bool, PrimitiveSerializer<bool>>("bool", 913258849);
            Add<byte, PrimitiveSerializer<byte>>("byte", 641654109);
            Add<short, PrimitiveSerializer<short>>("short", 790800769);
            Add<ushort, PrimitiveSerializer<ushort>>("ushort", 796556766);
            Add<int, PrimitiveSerializer<int>>("int", 749885637);
            Add<uint, PrimitiveSerializer<uint>>("uint", 870342263);
            Add<long, PrimitiveSerializer<long>>("long", 206998041);
            Add<ulong, PrimitiveSerializer<ulong>>("ulong", 114976024);
            Add<float, PrimitiveSerializer<float>>("float", 445383687);
            Add<double, PrimitiveSerializer<double>>("double", 512140433);
            Add<decimal, PrimitiveSerializer<decimal>>("decimal", 364525462);
            Add<char, PrimitiveSerializer<char>>("char", 430245554);
            Add<DateTime, DateTimeSerializer>("DateTime", 513672356);
            Add<TimeSpan, TimeSpanSerializer>("TimeSpan", 211582866);

            Add<string, PrimitiveClassSerializer<string>>("string", 370776660);
            Add<byte[], PrimitiveClassSerializer<byte[]>>("ByteArray", 368808522);
            Add<Type, PrimitiveClassSerializer<Type>>("Type", 751837157);

            Add<object, AbstractSerializer<object>>("object", 223470686);

            Add(typeof(List<>), typeof(ListSerializer<>), "List", 285816375);
            Add(typeof(LinkedList<>), typeof(LinkedListSerializer<>), "LinkedList", 259112330);
            Add(typeof(Dictionary<,>), typeof(DictionarySerializer<,>), "Dictionary", 325440653);
            Add(typeof(KeyValuePair<,>), typeof(KeyValuePairSerializer<,>), "kvp", 323068427);
        }

        public static void Add(
            Type serializedType, Type serializerType, string name, int id)
        {
            Add(new Definition(serializedType, serializerType, name, id));
        }

        public static void AddDeprecated<TSerialized>(
            string name, int id)
            where TSerialized : class, new()
        {
            Add<TSerialized, DeprecatedSerializer>(name, id);
        }

        public static void AddEmpty<TSerialized>(string name, int id)
            where TSerialized : class, new()
        {
            Add<TSerialized, EmptySerializer<TSerialized>>(name, id);
        }

        public static void AddEnum<TSerialized>(string name, int id)
            where TSerialized : struct
        {
            Add<TSerialized, PrimitiveSerializer<TSerialized>>(name, id);
        }

        public static void Add<TSerialized, TSerializer>(string name, int id)
            where TSerializer : ISerializer
        {
            Add(typeof(TSerialized), typeof(TSerializer), name, id);
        }

        public static void Add(Definition definition)
        {
            Assert.That(!_definitions.Contains(definition),
                "Found duplicate mapping for Serializer with name '" + definition + "'");

            Assert.That(_definitions.Where(x => x.Id == definition.Id).IsEmpty(),
                "Found duplicate mapping for Serializer with id '" + definition.Id + "'");

            Assert.That(_definitions.Where(x => x.SerializedType == definition.SerializedType).IsEmpty(),
                "Found duplicate mapping for Serializer with type '" + definition.SerializedType + "'");

            Assert.That(definition.Id > 0,
                "Serializer for type '{0}' must use an id greater than 0",
                definition.SerializedType.Name());

            _namedDefinitions.Add(definition.Name, definition);
            _typedDefinitions.Add(definition.SerializedType, definition);
            _indexToDefinition.Add(definition.Id, definition);
            _definitionToIndex.Add(definition, definition.Id);
        }

        // gets string name for the serialized type, used only by plaintext serialization
        public static string GetSerializedTypeName(Type serializedType)
        {
            List<string> nameBuilder = new List<string>();
            BuildSerializedTypeName(serializedType, nameBuilder);
            return string.Join(" ", nameBuilder.ToArray());
        }

        // parses the serialized type from the given string, used only by plaintext serialization
        public static Type ParseSerializedType(string name)
        {
            int index = 0;
            string[] nameElements = name.Split(_nameSplitCharacters, StringSplitOptions.RemoveEmptyEntries);
            return BuildSerialziedType(nameElements, ref index);
        }

        // writes serialized type id directly to writer, used only by binary serialization
        public static void WriteSerializedTypeId(Type serializedType, BinaryWriter writer)
        {
            if (serializedType.DerivesFrom<Type>())
            {
                serializedType = typeof(Type);
            }

            Definition definition = _typedDefinitions.TryGetValue(serializedType);

            if (definition != null)
            {
                writer.Write(_definitionToIndex[definition]);
            }
            else if (serializedType.IsArray)
            {
                Type elementType = serializedType.GetElementType();
                int dimensions = serializedType.GetArrayRank();
                writer.Write(_arrayBinaryId);
                writer.Write(dimensions);
                WriteSerializedTypeId(elementType, writer);
            }
            else if (serializedType.IsGenericType())
            {
                Type genericDefinition = serializedType.GetGenericTypeDefinition();

                definition = _typedDefinitions.TryGetValue(genericDefinition);
                if (definition != null)
                {
                    writer.Write(_definitionToIndex[definition]);

                    foreach (var argumentType in serializedType.GenericArguments())
                    {
                        WriteSerializedTypeId(argumentType, writer);
                    }
                }
            }
            else
            {
                throw Assert.CreateException(
                    "no serializer found for '{0}'", serializedType.Name());
            }
        }

        public static Type ReadSerializedTypeId(BinaryReader reader)
        {
            int index = reader.ReadInt32();
            return ReadSerializedTypeId(index, reader);
        }

        public static bool IsPrimitive(Type type)
        {
            return type.IsValueType() || type == typeof(string) || type == typeof(Type);
        }

        // reads seralized type id directly from reader, used only by binary serialization
        public static Type ReadSerializedTypeId(int index, BinaryReader reader)
        {
            if (index == _arrayBinaryId)
            {
                int dimensions = reader.ReadInt32();
                Type elementType = ReadSerializedTypeId(reader);
                // Note: there is a difference between MakeArrayType(1) and MakeArrayType()
                // See here: http://stackoverflow.com/a/7058050
                return dimensions == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(dimensions);
            }

            Definition definition = _indexToDefinition.TryGetValue(index);

            if (definition != null)
            {
                Type resultType = definition.SerializedType;

                if (resultType.IsGenericTypeDefinition())
                {
                    int argumentsCount = resultType.GenericArguments().Length;
                    Type[] argumentTypes = new Type[argumentsCount];
                    for (int i = 0; i < argumentsCount; ++i)
                    {
                        argumentTypes[i] = ReadSerializedTypeId(reader);
                    }
                    return resultType.MakeGenericType(argumentTypes);
                }

                return resultType;
            }

            throw Assert.CreateException(
                "no serializer found with id '{0}'", index);
        }

        // finds or creates a serializer for the given type
        public static ISerializer GetSerializer(Type serializedType)
        {
            ISerializer serializer = null;
            Definition definition = null;

            if (serializedType.DerivesFrom<Type>())
            {
                // Handle System.RuntimeType as a special case
                serializedType = typeof(Type);
            }

            if (_serializerInstances.TryGetValue(serializedType, out serializer))
            {
                return serializer;
            }

            if (_typedDefinitions.TryGetValue(serializedType, out definition))
            {
                serializer = (ISerializer)Activator.CreateInstance(definition.SerializerType);
                _serializerInstances.Add(serializedType, serializer);
                return serializer;
            }

            if (serializedType.IsArray)
            {
                Type elementType = serializedType.GetElementType();
                int dimensions = serializedType.GetArrayRank();
                serializer = new ArraySerializer(elementType, dimensions);
                _serializerInstances.Add(serializedType, serializer);
                return serializer;
            }

            if (serializedType.IsGenericType())
            {
                Type genericDefinition = serializedType.GetGenericTypeDefinition();
                Type[] genericArguments = serializedType.GenericArguments();
                if (_typedDefinitions.TryGetValue(genericDefinition, out definition))
                {
                    Type newSerializerType = definition.SerializerType.MakeGenericType(genericArguments);
                    serializer = (ISerializer)Activator.CreateInstance(newSerializerType);
                    _serializerInstances.Add(serializedType, serializer);
                    return serializer;
                }
            }

            throw Assert.CreateException(
                "no serializer found for type '{0}'", serializedType.Name());
        }

        static void BuildSerializedTypeName(Type type, List<string> nameBuilder)
        {
            if (type.DerivesFrom<Type>())
            {
                // Handle System.RuntimeType as a special case
                type = typeof(Type);
            }

            Definition definition = _typedDefinitions.TryGetValue(type);

            if (definition != null)
            {
                nameBuilder.Add(definition.Name);
            }
            else if (type.IsArray)
            {
                Type elementType = type.GetElementType();
                int dimensions = type.GetArrayRank();
                nameBuilder.Add(_arrayTextId);
                nameBuilder.Add(dimensions.ToString());
                BuildSerializedTypeName(elementType, nameBuilder);
            }
            else if (type.IsGenericType())
            {
                Type genericDefinition = type.GetGenericTypeDefinition();

                definition = _typedDefinitions.TryGetValue(genericDefinition);
                if (definition != null)
                {
                    nameBuilder.Add(definition.Name);
                    foreach (var argumentType in type.GenericArguments())
                    {
                        BuildSerializedTypeName(argumentType, nameBuilder);
                    }
                }
            }
            else
            {
                throw Assert.CreateException(
                    "no serializer found for '{0}'", type.Name());
            }
        }

        static Type BuildSerialziedType(string[] name, ref int index)
        {
            string primaryName = name[index++];

            if (primaryName == _arrayTextId)
            {
                int dimensions = int.Parse(name[index++]);
                Type elementType = BuildSerialziedType(name, ref index);
                // Note: there is a difference between MakeArrayType(1) and MakeArrayType()
                // See here: http://stackoverflow.com/a/7058050
                return dimensions == 1 ? elementType.MakeArrayType() : elementType.MakeArrayType(dimensions);
            }

            Definition definition = _namedDefinitions.TryGetValue(primaryName);

            if (definition != null)
            {
                Type resultType = definition.SerializedType;

                if (resultType.IsGenericTypeDefinition())
                {
                    int argumentsCount = resultType.GenericArguments().Length;
                    Type[] argumentTypes = new Type[argumentsCount];
                    for (int i = 0; i < argumentsCount; ++i)
                    {
                        Assert.That(index < name.Length, "not enough parameters to create '{0}' serializer", primaryName);
                        argumentTypes[i] = BuildSerialziedType(name, ref index);
                    }
                    return resultType.MakeGenericType(argumentTypes);
                }

                return resultType;
            }

            throw Assert.CreateException(
                "no serializer found with name '{0}'", primaryName);
        }

        public class Definition
        {
            public readonly string Name;
            public readonly int Id;
            public readonly Type SerializedType;
            public readonly Type SerializerType;

            public Definition(Type serializedType, Type serializerType, string name, int id)
            {
                Name = name;
                Id = id;
                SerializedType = serializedType;
                SerializerType = serializerType;
            }
        }
    }
}
