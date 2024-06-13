using System.Collections.Generic;

namespace SpaceAce.Gameplay.Shooting.Ammo
{
    public interface IEMPTargetProvider
    {
        IEnumerable<IEMPTarget> GetTargets();
    }
}