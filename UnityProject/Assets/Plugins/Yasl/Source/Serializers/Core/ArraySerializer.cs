using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ModestTree.Util
{
    public class ArraySerializer : ISerializer
    {
        readonly Type _elementType;
        //readonly int _arrayDimensions;

        public ArraySerializer(Type elementType, int arrayDimensions)
        {
            _elementType = elementType;
            //_arrayDimensions = arrayDimensions;
        }

        public void SerializeConstructor(ref object value, ISerializationWriter writer)
        {
            Array array = (Array)value;

            int[] lengths = new int[array.Rank];
            for (int i = 0; i < array.Rank; ++i)
            {
                lengths[i] = array.GetLength(i);
            }

            writer.WriteSet(typeof(int), "Length", lengths);
        }

        public void SerializeContents(ref object value, ISerializationWriter writer)
        {
            writer.WriteSet(_elementType, SerializationConstants.DefaultValueItemName, (Array)value);
        }

        public void SerializeBacklinks(ref object value, ISerializationWriter writer)
        {
        }

        public void DeserializeConstructor(out object value, int version, ISerializationReader reader)
        {
            int[] lengths = reader.ReadSet<int>("Length").ToArray();
            value = Array.CreateInstance(_elementType, lengths);
        }

        bool RecursiveAssignArray(IEnumerator valueEnumerator, Array array, int[] index, int depth)
        {
            if (depth == array.Rank)
            {
                if (valueEnumerator.MoveNext())
                {
                    array.SetValue(valueEnumerator.Current, index);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                for (index[depth] = 0; index[depth] < array.GetLength(depth); index[depth] += 1)
                {
                    if (!RecursiveAssignArray(valueEnumerator, array, index, depth + 1))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public void DeserializeContents(ref object value, int version, ISerializationReader reader)
        {
            Array array = (Array)value;

            var enumerator = reader.ReadSet(_elementType, SerializationConstants.DefaultValueItemName, false).GetEnumerator();

            if (array.Rank == 1)
            {
                for (int i = 0; i < array.Length && enumerator.MoveNext(); ++i)
                {
                    array.SetValue(enumerator.Current, i);
                }
            }
            else
            {
                int[] index = new int[array.Rank];
                RecursiveAssignArray(enumerator, array, index, 0);
            }

            while (enumerator.MoveNext())
            {
                // ensures all enumerated items are read
            }
        }

        public void DeserializeBacklinks(ref object value, int version, ISerializationReader reader)
        {
        }

        public void DeserializationComplete(ref object value, int version)
        {
        }
    }
}
