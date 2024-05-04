using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpaceAce.Gameplay.Meteors
{
    public readonly struct MeteorWave : IEnumerable<MeteorWaveSlot>
    {
        private readonly IEnumerable<MeteorWaveSlot> _slots;

        public readonly bool MeteorShower { get; }
        public int Size => _slots.Count();

        public MeteorWave(IEnumerable<MeteorWaveSlot> slots, bool meteorShower)
        {
            _slots = slots ?? throw new ArgumentNullException();
            MeteorShower = meteorShower;
        }

        public float GetDuration()
        {
            float duration = 0f;

            foreach (var slot in _slots) duration += slot.SpawnDelay;

            return duration;
        }

        #region interfaces

        public IEnumerator<MeteorWaveSlot> GetEnumerator() => _slots.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}