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
        private readonly Dictionary<ProjectileHitEffectSkin, Stack<CachedParticleSystem>> _objtectPools = new();
        private readonly Dictionary<ProjectileHitEffectSkin, Transform> _objectPoolsAnchors = new();
        private readonly GameObject _masterAnchor = new("Projectile hit effects pools");

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
                GameObject anchor = new($"Anchor of {skin.ToString().ToLower()}");
                anchor.transform.parent = _masterAnchor.transform;

                _objectPoolsAnchors.Add(skin, anchor.transform);
            }
        }

        public async UniTask CreateAsync(ProjectileHitEffectSkin skin, Vector3 position)
        {
            CachedParticleSystem hitEffect = InstantiateHitEffect(skin, position);
            await AwaitHitEffectToPlayAsync(hitEffect);
            Release(hitEffect, skin);
        }

        private CachedParticleSystem InstantiateHitEffect(ProjectileHitEffectSkin skin, Vector3 position)
        {
            if (_objtectPools.TryGetValue(skin, out Stack<CachedParticleSystem> stack) == true &&
                stack.TryPop(out CachedParticleSystem system) == true)
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

                return new(instance, transform, pauser);
            }

            throw new Exception($"Hit effect of a requested skin ({skin}) doesn't exist!");
        }

        private async UniTask AwaitHitEffectToPlayAsync(CachedParticleSystem instance)
        {
            float timer = 0f;

            while (timer < instance.Pauser.EffectDuration)
            {
                timer += Time.deltaTime;

                while (_gamePauser.Paused == true) await UniTask.Yield();

                await UniTask.Yield();
            }
        }

        private void Release(CachedParticleSystem instance, ProjectileHitEffectSkin skin)
        {
            instance.Object.SetActive(false);
            instance.Transform.parent = _objectPoolsAnchors[skin].transform;
            instance.Transform.position = Vector3.zero;

            if (_objtectPools.TryGetValue(skin, out Stack<CachedParticleSystem> stack) == true)
            {
                stack.Push(instance);
            }
            else
            {
                Stack<CachedParticleSystem> newStack = new();
                newStack.Push(instance);

                _objtectPools.Add(skin, newStack);
            }
        }
    }
}