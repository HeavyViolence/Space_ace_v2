using Cysharp.Threading.Tasks;

using System;
using System.Collections.Generic;

using UnityEngine;

using Zenject;

namespace SpaceAce.Main.Factories
{
    public sealed class ProjectileHitEffectFactory
    {
        private readonly DiContainer _diContainer;
        private readonly GamePauser _gamePauser;
        private readonly Dictionary<ProjectileHitEffectSkin, GameObject> _hitEffectPrefabs = new();
        private readonly Dictionary<ProjectileHitEffectSkin, GameObject> _hitEffectAnchors = new();
        private readonly Dictionary<ProjectileHitEffectSkin, Stack<CachedParticleSystem>> _hitEffectsPool = new();
        private readonly GameObject _hitEffectsMasterAnchor = new("Projectile hit effects master anchor");

        public ProjectileHitEffectFactory(DiContainer diContainer,
                                          GamePauser gamePauser,
                                          IEnumerable<ProjectileHitEffectFactoryConfigSlot> hitEffects)
        {
            _diContainer = diContainer ?? throw new ArgumentNullException();
            _gamePauser = gamePauser ?? throw new ArgumentNullException();

            if (hitEffects is null) throw new ArgumentNullException();

            foreach (var hitEffect in hitEffects)
            {
                _hitEffectPrefabs.Add(hitEffect.Skin, hitEffect.Prefab);

                GameObject anchor = new($"Anchor of: {hitEffect.Skin}");
                anchor.transform.parent = _hitEffectsMasterAnchor.transform;

                _hitEffectAnchors.Add(hitEffect.Skin, anchor);
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
            if (_hitEffectsPool.TryGetValue(skin, out Stack<CachedParticleSystem> stack) == true && stack.Count > 0)
            {
                CachedParticleSystem hitEffect = stack.Pop();

                hitEffect.Instance.SetActive(true);
                hitEffect.Instance.transform.parent = null;
                hitEffect.Instance.transform.position = position;

                return hitEffect;
            }
            
            if (_hitEffectPrefabs.TryGetValue(skin, out GameObject hitPrefab) == true)
            {
                GameObject instance = _diContainer.InstantiatePrefab(hitPrefab);

                if (instance.TryGetComponent(out ParticleSystemPauser pauser) == true) return new(instance, pauser);
                else throw new MissingComponentException($"Hit prefab is missing {typeof(ParticleSystemPauser)}!");
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
            if (instance is null) throw new ArgumentNullException();

            instance.Instance.SetActive(false);
            instance.Instance.transform.parent = _hitEffectAnchors[skin].transform;
            instance.Instance.transform.position = Vector3.zero;

            if (_hitEffectsPool.TryGetValue(skin, out Stack<CachedParticleSystem> stack) == true)
            {
                stack.Push(instance);
            }
            else
            {
                Stack<CachedParticleSystem> newStack = new();
                newStack.Push(instance);

                _hitEffectsPool.Add(skin, newStack);
            }
        }
    }
}