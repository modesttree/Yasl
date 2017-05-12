using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ModestTree.Util
{
    public static class SerializerExtensions
    {
        public static void Write<T>(this ISerializationWriter writer, string name, T value)
        {
            writer.Write(typeof(T), name, value);
        }

        public static T Read<T>(this ISerializationReader reader, string name, bool allowCircularDependencies = false)
        {
            return (T)reader.Read(typeof(T), name, allowCircularDependencies);
        }

        public static IEnumerable<T> ReadSet<T>(this ISerializationReader reader, string itemName, bool allowCircularDependencies = false)
        {
            return reader.ReadSet(typeof(T), itemName, allowCircularDependencies).Cast<T>();
        }

        public static void WriteSet<T>(this ISerializationWriter writer, string itemName, IEnumerable set)
        {
            writer.WriteSet(typeof(T), itemName, set);
        }

        public static T ReadPrimitive<T>(this ISerializationReader reader)
        {
            return (T)reader.ReadPrimitive(typeof(T));
        }

        public static void WritePrimitive<T>(this ISerializationWriter writer, T value)
        {
            writer.WritePrimitive(typeof(T), value);
        }
    }
}
