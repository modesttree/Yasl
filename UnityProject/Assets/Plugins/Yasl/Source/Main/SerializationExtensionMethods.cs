using System;
using System.Xml.Linq;

namespace ModestTree.Util
{
    public static class SerializationExtensionMethods
    {
        // Same as Element() except throws an exception if not found
        // rather than returning null
        // Easier to debug
        public static XElement GetElement(this XElement elem, string name)
        {
            var result = elem.Element(name);

            if (result == null)
            {
                throw new SerializationException(
                    "Unable to find element with name '{0}'".Fmt(name));
            }

            return result;
        }

        // Same as Attribute() except throws an exception if not found
        // rather than returning null
        // Easier to debug
        public static XAttribute GetAttribute(this XElement elem, string name)
        {
            var result = elem.Attribute(name);

            if (result == null)
            {
                throw new SerializationException(
                    "Unable to find attribute with name '{0}'".Fmt(name));
            }

            return result;
        }
    }
}
