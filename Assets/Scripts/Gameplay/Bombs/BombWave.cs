using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SpaceAce.Gameplay.Bombs
{
    public sealed record BombWave : IEnumerable<BombWaveSlot>
    {
        private readonly IEnumerable<BombWaveSlot> _slots;

        public int Size => _slots.Count();

        public BombWave(IEnumerable<BombWaveSlot> slots)
        {
            _slots = slots ?? throw new ArgumentNullException();
        }

        public float GetDuration()
        {
            float duration = 0f;

            foreach (var slot in _slots) duration += slot.SpawnDelay;

            return duration;
        }

        #region interfaces

        public IEnumerator<BombWaveSlot> GetEnumerator() => _slots.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}