using System.Collections.Generic;

namespace SpaceAce.Gameplay.Effects
{
    public interface IEMPTargetProvider
    {
        IEnumerable<IEMPTarget> GetTargets();
    }
}