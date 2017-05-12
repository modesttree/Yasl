using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace ModestTree.Util
{
    public class EmptySerializer<T> : SerializerSimple<T> where T : class, new()
    {
        public EmptySerializer()
        {
        }
    }
}
