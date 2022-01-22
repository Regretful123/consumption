using System;
using UnityEngine;

public interface IState
{
    void OnEnter();
    void Execute();
    void OnExit();
}

[Serializable]
public abstract class State : MonoBehaviour, IState 
{
    public Action onEnter;
    public Action onExit;
    protected StateMachine _stateMachine;
    public State( StateMachine stateMachine ) => _stateMachine = stateMachine;
    public StateMachine stateMachine { get => _stateMachine; }
    public virtual void Init( StateMachine machine ) => this._stateMachine = machine;
    public virtual void OnEnter() => onEnter?.Invoke();
    public virtual void OnExit() => onExit?.Invoke();
    public abstract void Execute();
}
