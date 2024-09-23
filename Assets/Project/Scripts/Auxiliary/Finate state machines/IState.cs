using System;

namespace SpaceAce.Auxiliary.FinateStateMachines
{
    public interface IState : IEquatable<IState>
    {
        Guid ID { get; }

        void OnStateEnter();
        void OnStateUpdate();
        void OnStateFixedUpdate();
        void OnStateExit();
    }
}