using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public class StateMachine<State>
{
    public delegate void ProcessStateAction(double delta);
    class Transition
    {
        public int _Priority { get; set; }
        public State _toState { get; set; }
        public Func<bool> _transitionEvaluator { get; set; }

        public Transition(State toState, Func<bool> transitionEvaluator, int priority)
        {
            _Priority = priority;
            _toState = toState;
            _transitionEvaluator = transitionEvaluator;
        }
    }

    private State _curState;
    private IDictionary<State, ProcessStateAction> _stateProcessActions;
    private IDictionary<State, Action> _stateEnterActions;
    private IDictionary<State, Action> _stateExitActions;
    private IDictionary<State, List<Transition>> _stateTransitions;
    private HashSet<int> _eventSet;

    public StateMachine(State initialState)
    {
        _curState = initialState;
        _stateProcessActions = new Dictionary<State, ProcessStateAction>();
        _stateEnterActions = new Dictionary<State, Action>();
        _stateExitActions = new Dictionary<State, Action>();
        _stateTransitions = new Dictionary<State, List<Transition>>();
        _eventSet = new HashSet<int>();
    }

    public State getCurrentState()
    {
        return _curState;
    }

    public void addStateProcessAction(State state, ProcessStateAction action)
    {
        _stateProcessActions[state] = action;
    }

    public void addStateProcessAction(State state, Action action)
    {
        _stateProcessActions[state] = (double delta) => action();
    }

    public void addStateEnterAction(State state, Action action)
    {
        _stateEnterActions[state] = action;
    }

    public void addStateExitAction(State state, Action action)
    {
        _stateExitActions[state] = action;
    }

    public void addStateTransition(
        State fromState,
        State toState,
        Func<bool> transitionFunc,
        int priority = 0
    )
    {
        if (!_stateTransitions.ContainsKey(fromState))
        {
            _stateTransitions[fromState] = new List<Transition>();
        }
        _stateTransitions[fromState].Add(new Transition(toState, transitionFunc, priority));
    }

    public void addStateTransition(State fromState, State toState, int eventID, int priority = 0)
    {
        addStateTransition(fromState, toState, () => _eventSet.Contains(eventID), priority);
    }

    public void addStateTransitions(
        State[] fromStates,
        State toState,
        Func<bool> transitionFunc,
        int priority = 0
    )
    {
        foreach (var fromState in fromStates)
        {
            addStateTransition(fromState, toState, transitionFunc, priority);
        }
    }

    public void addStateTransitions(
        State[] fromStates,
        State toState,
        int eventID,
        int priority = 0
    )
    {
        foreach (var fromState in fromStates)
        {
            addStateTransition(fromState, toState, eventID, priority);
        }
    }

    public void ProcessState(double delta)
    {
        RunStateProcessAction(delta);
        RunStateTransitions();
        _eventSet.Clear();
    }

    public void SendEvent(int eventID)
    {
        _eventSet.Add(eventID);
    }

    private void RunStateProcessAction(double delta)
    {
        if (_stateProcessActions.TryGetValue(_curState, out ProcessStateAction action))
        {
            action(delta);
        }
    }

    private void RunStateEnterAction(State state)
    {
        if (_stateEnterActions.TryGetValue(state, out Action action))
        {
            action();
        }
    }

    private void RunStateExitAction(State state)
    {
        if (_stateExitActions.TryGetValue(state, out Action action))
        {
            action();
        }
    }

    private void RunStateTransitions()
    {
        var oldState = _curState;
        if (_stateTransitions.TryGetValue(_curState, out List<Transition> transitions))
        {
            int highestPriority = int.MinValue;
            foreach (var transition in transitions)
            {
                if (transition._transitionEvaluator() && transition._Priority > highestPriority)
                {
                    _curState = transition._toState;
                    highestPriority = transition._Priority;
                }
            }
        }
        if (!_curState.Equals(oldState))
        {
            RunStateExitAction(oldState);
            RunStateEnterAction(_curState);
        }
    }
}
