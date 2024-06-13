using SpaceAce.Main.Factories.AmmoFactories;

using System.Collections.Generic;

using UnityEngine;

namespace SpaceAce.Gameplay.Players
{
    [CreateAssetMenu(fileName = "Player starting items dispenser config",
                     menuName = "Space ace/Configs/Player/Player starting items dispenser config")]
    public sealed class PlayerStartingItemsDispenserConfig : ScriptableObject
    {
        #region starting credits

        public const float MinStartingCredits = 0f;
        public const float MaxStartingCredits = 1_000f;

        [SerializeField, Range(MinStartingCredits, MaxStartingCredits), Space]
        float _startingCredits = MinStartingCredits;

        public float StartingCredits => _startingCredits;

        #endregion

        #region starting ammo

        [SerializeField, Space]
        private List<PlayerStartingAmmoConfig> _startingAmmo;

        public IEnumerable<AmmoFactoryRequest> GetStartingAmmoRequests()
        {
            List<AmmoFactoryRequest> requests = new(_startingAmmo.Count);
            foreach (var ammo in _startingAmmo) requests.Add(ammo.Request);

            return requests;
        }

        #endregion
    }
}