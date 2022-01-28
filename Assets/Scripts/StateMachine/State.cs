using System;
using System.Collections;
using UnityEngine;

public interface IState
{
    IEnumerator Init();
    void Execute();
    void FixedExecute();
}

public abstract class State : IState 
{
    protected StateMachine machine;
    public State( StateMachine stateMachine ) => machine = stateMachine;
    public StateMachine stateMachine => machine;
    public virtual IEnumerator Init() { yield break; }
    public virtual void Execute() { }
    public virtual void FixedExecute() { }
}
