using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories.ProjectileHitEffectFactories
{
    public sealed class ProjectileHitEffectFactory
    {
        private readonly DiContainer _diContainer;
        private readonly GamePauser _gamePauser;
        private readonly Dictionary<ProjectileHitEffectSkin, GameObject> _hitEffectPrefabs = new();
        private readonly Dictionary<ProjectileHitEffectSkin, Stack<ParticleSystemCache>> _objtectPools = new();
        private readonly Dictionary<ProjectileHitEffectSkin, Transform> _objectPoolsAnchors = new();
        private readonly Transform _masterAnchor = new GameObject("Projectile hit effects pools").transform;

        public ProjectileHitEffectFactory(DiContainer diContainer,
                                          GamePauser gamePauser,
                                          ProjectileHitEffectFactoryConfig config)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();

            if (config == null) throw new ArgumentNullException();
            _hitEffectPrefabs = new(config.GetHitEffectsPrefabs());

            BuildObjectPoolsAnchors();
        }

        private void BuildObjectPoolsAnchors()
        {
            Array skins = Enum.GetValues(typeof(ProjectileHitEffectSkin));

            foreach (ProjectileHitEffectSkin skin in skins)
            {
                Transform anchor = new GameObject($"{skin}").transform;
                anchor.parent = _masterAnchor;

                _objectPoolsAnchors.Add(skin, anchor);
            }
        }

        public async UniTask CreateAsync(ProjectileHitEffectSkin skin, Vector3 position)
        {
            ParticleSystemCache hitEffect = InstantiateHitEffect(skin, position);
            await AwaitHitEffectToPlayAsync(hitEffect);
            Release(skin, hitEffect);
        }

        private ParticleSystemCache InstantiateHitEffect(ProjectileHitEffectSkin skin, Vector3 position)
        {
            if (_objtectPools.TryGetValue(skin, out Stack<ParticleSystemCache> stack) == true &&
                stack.TryPop(out ParticleSystemCache system) == true)
            {
                system.Object.SetActive(true);
                system.Transform.parent = null;
                system.Transform.position = position;

                return system;
            }
            
            if (_hitEffectPrefabs.TryGetValue(skin, out GameObject hitPrefab) == true)
            {
                GameObject instance = _diContainer.InstantiatePrefab(hitPrefab);

                if (instance.TryGetComponent(out Transform transform) == false) throw new MissingComponentException($"{typeof(Transform)}");
                if (instance.TryGetComponent(out ParticleSystemPauser pauser) == false) throw new MissingComponentException($"{typeof(ParticleSystemPauser)}!");

                transform.parent = null;
                transform.position = position;

                return new(instance, pauser);
            }

            throw new Exception($"Hit effect of a requested skin ({skin}) doesn't exist!");
        }

        private async UniTask AwaitHitEffectToPlayAsync(ParticleSystemCache instance)
        {
            float timer = 0f;

            while (timer < instance.Pauser.EffectDuration)
            {
                timer += Time.deltaTime;

                while (_gamePauser.Paused == true) await UniTask.Yield();

                await UniTask.Yield();
            }
        }

        private void Release(ProjectileHitEffectSkin skin, ParticleSystemCache instance)
        {
            if (instance is null) throw new ArgumentNullException();

            instance.Object.SetActive(false);
            instance.Transform.parent = _objectPoolsAnchors[skin].transform;
            instance.Transform.position = Vector3.zero;

            if (_objtectPools.TryGetValue(skin, out Stack<ParticleSystemCache> stack) == true)
            {
                stack.Push(instance);
            }
            else
            {
                Stack<ParticleSystemCache> newStack = new();
                newStack.Push(instance);

                _objtectPools.Add(skin, newStack);
            }
        }
    }
}