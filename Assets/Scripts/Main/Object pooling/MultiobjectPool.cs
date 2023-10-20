using Cysharp.Threading.Tasks;
using SpaceAce.Architecture;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace SpaceAce.Main.ObjectPooling
{ 
    public sealed class MultiobjectPool : IDisposable
    {
        private readonly Dictionary<string, ObjectPool<GameObject>> _multiobjectPool = new();
        private readonly Dictionary<string, GameObject> _poolAnchors = new();

        private readonly GamePauser _gamePauser = null;
        private readonly GameObject _masterAnchor = null;

        public MultiobjectPool(GamePauser gamePauser)
        {
            _gamePauser = gamePauser ?? throw new ArgumentNullException(nameof(gamePauser),
                $"Attempted to pass an empty {typeof(GamePauser)}!");

            _masterAnchor = new GameObject("Multiobject pool");
        }

        public void EnsureObjectPoolExistence(string anchorName, GameObject sample)
        {
            if (string.IsNullOrEmpty(anchorName) || string.IsNullOrWhiteSpace(anchorName))
                throw new ArgumentNullException(nameof(anchorName), "Attempted to pass an empty object pool anchor name!");

            if (sample == null) throw new ArgumentNullException(nameof(sample), "Attempted to pass an empty object prefab!");

            if (_multiobjectPool.ContainsKey(anchorName) == false)
            {
                GameObject poolAnchor = new($"Object pool of: {anchorName}");
                poolAnchor.transform.parent = _masterAnchor.transform;
                _poolAnchors.Add(anchorName, poolAnchor);

                ObjectPool<GameObject> newPool = new(OnCreate(sample), OnGet, OnRelease, OnDestroy);
                _multiobjectPool.Add(anchorName, newPool);
            }
        }

        public void ClearObjectPool(string anchorName)
        {
            if (_multiobjectPool.TryGetValue(anchorName, out var pool) == true)
            {
                pool.Clear();
                _multiobjectPool.Remove(anchorName);

                if (_poolAnchors.TryGetValue(anchorName, out var anchor) == true)
                {
                    _poolAnchors.Remove(anchorName);
                    UnityEngine.Object.Destroy(anchor);
                }
            }
            else
            {
                throw new Exception($"Attempted to clear a non-existing object pool: {anchorName}!");
            }
        }

        public GameObject GetObject(string anchorName)
        {
            if (_multiobjectPool.TryGetValue(anchorName, out var pool) == true)
            {
                GameObject obj = pool.Get();

                if (obj.transform.parent == null &&
                    _poolAnchors.TryGetValue(anchorName, out var anchor) == true)
                {
                    obj.transform.parent = anchor.transform;
                }

                return obj;
            }
            else
            {
                throw new Exception($"Attempted to get an object from a non-existing object pool: {anchorName}!");
            }
        }

        public async UniTaskVoid ReleaseObject(string anchorName, GameObject member, Func<bool> condition, Action releaseAction = null, float delay = 0f)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (member.activeInHierarchy == false) return;

            while (condition() == false || _gamePauser.Paused == true) await UniTask.NextFrame();

            if (delay > 0f)
            {
                float timer = 0f;

                while (timer < delay)
                {
                    timer += Time.deltaTime;

                    await UniTask.NextFrame();
                    while (_gamePauser.Paused == true) await UniTask.NextFrame();
                }
            }

            if (member.activeInHierarchy == true)
            {
                if (_multiobjectPool.TryGetValue(anchorName, out var pool) == true) pool.Release(member);
                else throw new Exception($"Attempted to release an object to a non-existing object pool: {anchorName}!");
            }

            releaseAction?.Invoke();
        }

        public void Dispose()
        {
            foreach (var element in _multiobjectPool) element.Value.Dispose();
            _multiobjectPool.Clear();

            foreach (var element in _poolAnchors) UnityEngine.Object.Destroy(element.Value);
            _poolAnchors.Clear();
        }

        private Func<GameObject> OnCreate(GameObject prefab) => delegate ()
        {
            return UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        };

        private Action<GameObject> OnGet => delegate (GameObject obj)
        {
            obj.SetActive(true);
        };

        private Action<GameObject> OnRelease => delegate (GameObject obj)
        {
            obj.SetActive(false);
            obj.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        };

        private Action<GameObject> OnDestroy => delegate (GameObject obj)
        {
            UnityEngine.Object.Destroy(obj);
        };
    }
}