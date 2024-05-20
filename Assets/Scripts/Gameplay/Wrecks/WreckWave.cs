using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpaceAce.Gameplay.Wrecks
{
    public sealed record WreckWave : IEnumerable<WreckWaveSlot>
    {
        private readonly IEnumerable<WreckWaveSlot> _slots;

        public bool WreckShower { get; }
        public int Size => _slots.Count();

        public WreckWave(IEnumerable<WreckWaveSlot> slots, bool wreckShower)
        {
            _slots = slots ?? throw new ArgumentNullException();
            WreckShower = wreckShower;
        }

        public float GetDuration()
        {
            float duration = 0f;

            foreach (var slot in _slots) duration += slot.SpawnDelay;

            return duration;
        }

        #region interfaces

        public IEnumerator<WreckWaveSlot> GetEnumerator() => _slots.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}