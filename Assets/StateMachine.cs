using System;
using System.Collections.Generic;

public class StateMachine
{
    public string CurrentStateName { get; private set; }
    public string TransitingToState { get; private set; }

    private State _CurrentState = null;

    private Dictionary<string, State> _states = new Dictionary<string, State>();

    public void AddState(string name, State state)
    {
        _states.Add(name, state);
    }

    public void ChangeState(string stateName)
    {
        TransitingToState = stateName;
    }

    public void ChangeState<T>(string stateName, T parameter)
    {
        if (_states[stateName] is IStateParameter<T> stateWithParameter)
        {
            stateWithParameter.SetParameter(parameter);
        }
        else
        {
            throw new Exception($"Failed to move to state. The state {stateName} dosn't support parameter of type {typeof(T)}");
        }

        ChangeState(stateName);
    }

    public void RunState()
    {
        if (string.IsNullOrEmpty(TransitingToState) == false)
        {
            _CurrentState?.ExitState();
            CurrentStateName = TransitingToState;
            TransitingToState = string.Empty;
            _CurrentState = _states[CurrentStateName];
            _CurrentState.StartState();
        }

        _CurrentState?.UpdateState();
    }
}

public interface IStateParameter<T>
{
    void SetParameter(T nextValue);
}

public class State
{
    public string Name { get; set; }

    public virtual void StartState()
    {
    }

    public virtual void UpdateState()
    {
    }

    public virtual void ExitState()
    {
    }
}

public class ActionState : State
{
    public Action<ActionStateType> StateAction;

    public override void StartState()
    {
        base.StartState();
        StateAction?.Invoke(ActionStateType.Start);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        StateAction?.Invoke(ActionStateType.Update);
    }

    public override void ExitState()
    {
        base.ExitState();
        StateAction?.Invoke(ActionStateType.Exit);
    }
}

public class ActionState<T> : State, IStateParameter<T>
{
    public Action<ActionStateType, T> StateAction;
    T _parameterInfo = default(T);
    T _nextParameterInfo = default(T);
    bool _wasNextParameterSet = false;

    public override void StartState()
    {
        base.StartState();
        _parameterInfo = _nextParameterInfo;
        _nextParameterInfo = default(T);

        if (_wasNextParameterSet == false)
        {
            throw new Exception($"Cannot enter an IStateParameter<{typeof(T)}> without parameters");
        }

        _wasNextParameterSet = false;
        StateAction?.Invoke(ActionStateType.Start, _parameterInfo);
    }

    public override void UpdateState()
    {
        base.UpdateState();
        StateAction?.Invoke(ActionStateType.Update, _parameterInfo);
    }

    public override void ExitState()
    {
        base.ExitState();
        StateAction?.Invoke(ActionStateType.Exit, _parameterInfo);
        _nextParameterInfo = default(T);
    }

    public void SetParameter(T nextValue)
    {
        _nextParameterInfo = nextValue;
        _wasNextParameterSet = true;
    }
}

public enum ActionStateType
{
    Start,
    Update,
    Exit
}