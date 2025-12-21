using System;

namespace Serializator
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class GenerateBinarySerializerAttribute : Attribute
    {

    }
}
