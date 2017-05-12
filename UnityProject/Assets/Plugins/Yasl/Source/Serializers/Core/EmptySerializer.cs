using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Linq;

namespace Yasl
{
    public class EmptySerializer<T> : SerializerSimple<T> where T : class, new()
    {
        public EmptySerializer()
        {
        }
    }
}
