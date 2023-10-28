using NaughtyAttributes;

using SpaceAce.Architecture;

using System;

using UnityEngine;

namespace SpaceAce.Main.ObjectPooling
{
    [CreateAssetMenu(fileName = "Object pool entry",
                     menuName = "Space ace/Configs/Object pool entry")]
    public sealed class ObjectPoolEntry : ScriptableObject, IEquatable<ObjectPoolEntry>
    {
        [SerializeField]
        private GameObject _prefab = null;

        [SerializeField]
        private string _anchorName = string.Empty;

        private static readonly CachedService<MultiobjectPool> s_multiobjectPool = new();

        public GameObject Prefab => _prefab;
        public string AnchorName => _anchorName;

        public void EnsureObjectPoolExistence() => s_multiobjectPool.Access.EnsureObjectPoolExistence(AnchorName, Prefab);

        [Button("Set proper name")]
        private void SetProperName()
        {
            if (_prefab == null) return;
            _anchorName = _prefab.name.ToLower();
        }

        [Button("Clear name")]
        private void ClearName()
        {
            if (string.IsNullOrEmpty(_anchorName) == true) return;
            _anchorName = string.Empty;
        }

        #region interfaces

        public override bool Equals(object obj) => Equals(obj as ObjectPoolEntry);

        public bool Equals(ObjectPoolEntry other) => other != null && other.AnchorName.Equals(AnchorName);

        public override int GetHashCode() => AnchorName.GetHashCode();

        public override string ToString() => $"Object pool entry of {Prefab.name} as {AnchorName}.";

        #endregion
    }
}