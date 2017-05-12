using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yasl;

public class Foo
{
    public string Bar;
    public Vector3 Qux;
}

public class FooSerializer : Serializer<Foo>
{
    public override void SerializeConstructor(Foo value, ISerializationWriter writer)
    {
        writer.Write<string>("Bar", value.Bar);
        writer.Write<Vector3>("Qux", value.Qux);
    }

    public override Foo DeserializeConstructor(int version, ISerializationReader reader)
    {
        var value = new Foo();

        value.Bar = reader.Read<string>("Bar");
        value.Qux = reader.Read<Vector3>("Qux");

        return value;
    }
}

public class Example : MonoBehaviour
{
    public void Start()
    {
        SerializerRegistry.Add<Foo, FooSerializer>("Foo", 329702953);

        var foo = new Foo()
        {
            Bar = "asdf",
            Qux = new Vector3(1, 2, 3)
        };

        var xmlStr = SerializationManager.SerializeToXml("Foo", foo).ToString();
        Debug.Log("Xml: " + xmlStr);
    }
}
