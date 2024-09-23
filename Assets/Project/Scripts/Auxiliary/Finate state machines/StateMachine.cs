using System;
using System.Collections.Generic;

namespace SpaceAce.Auxiliary.FinateStateMachines
{
    public abstract class StateMachine
    {
        private readonly Dictionary<Guid, List<Transition>> _availableTransitions = new();

        private IState _currentState;
        private List<Transition> _currentStateTransitions = new();

        private IState _defaultState;
        private readonly HashSet<Func<bool>> _defaultStateConditions = new();

        private IState _previousState;

        public Guid CurrentStateID => _currentState.ID;
        public Guid DefaultStateID => _defaultState.ID;
        public Guid PreviousStateID => _previousState.ID;

        public abstract void OnSetup();

        public abstract void OnInitialize();

        public void OnUpdate()
        {
            Transition t = GetTransition();
            if (TransitionIsValid(t) == true) SetCurrentState(t.To);
            _currentState?.OnStateUpdate();
        }

        public void OnFixedUpdate()
        {
            _currentState?.OnStateFixedUpdate();
        }

        protected void AddTransition(IState from, IState to, Func<bool> condition)
        {
            Transition t = new(to, condition);

            if (_availableTransitions.TryGetValue(from.ID, out List<Transition> transitions) == true)
            {
                if (transitions.Contains(t) == true)
                {
                    throw new ArgumentException("Attempted to add already existing transition!");
                }
                else
                {
                    transitions.Add(t);
                }
            }
            else
            {
                _availableTransitions.Add(from.ID, new List<Transition>() { t });
            }
        }

        protected void SetDefaultState(IState state) => _defaultState = state;

        protected void SetInitialState(IState state)
        {
            _currentState = state;
            _currentStateTransitions = _availableTransitions[state.ID];
        }

        protected void AddDefaultStateCondition(Func<bool> condition) =>
            _defaultStateConditions.Add(condition);

        private Transition GetTransition()
        {
            foreach (Transition t in _currentStateTransitions)
                if (t.Condition() == true)
                    return t;

            foreach (Func<bool> condition in _defaultStateConditions)
                if (condition() == true)
                    return new Transition(_defaultState, condition);

            return Transition.Default;
        }

        private bool TransitionIsValid(Transition t) =>
            t.Equals(Transition.Default) == false && t.To.Equals(_currentState) == false;

        private void SetCurrentState(IState state)
        {
            _currentState.OnStateExit();

            _previousState = _currentState;
            _currentState = state;
            _currentStateTransitions = _availableTransitions[state.ID];

            _currentState.OnStateEnter();
        }
    }
}