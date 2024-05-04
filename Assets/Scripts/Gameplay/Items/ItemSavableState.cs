using Newtonsoft.Json.Serialization;

using SpaceAce.Main.Factories.SavedItemsFactories;

using System;

namespace SpaceAce.Gameplay.Items
{
    public abstract class ItemSavableState : ISerializationBinder
    {
        public Size Size { get; }
        public Quality Quality { get; }
        public float Price { get; }

        public ItemSavableState(Size size, Quality quality, float price)
        {
            Size = size;
            Quality = quality;
            Price = price;
        }

        public abstract IItem Recreate(SavedItemsServices services);

        #region interfaces

        public Type BindToType(string assemblyName, string typeName) =>
            Type.GetType($"{typeName}, {assemblyName}");

        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = serializedType.Assembly.FullName;
            typeName = serializedType.FullName;
        }

        #endregion
    }
}