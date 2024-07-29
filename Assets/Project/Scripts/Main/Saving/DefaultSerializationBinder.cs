using Newtonsoft.Json.Serialization;

using System;

namespace SpaceAce.Main.Saving
{
    public sealed class DefaultSerializationBinder : ISerializationBinder
    {
        public Type BindToType(string assemblyName, string typeName) => Type.GetType($"{typeName}, {assemblyName}");

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName;
        }
    }
}