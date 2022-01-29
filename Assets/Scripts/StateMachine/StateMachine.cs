using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    [SerializeField] protected State currentState;
    public State GetState() => currentState;
    public virtual void SetState( State newState ) => StartCoroutine((currentState = newState).Init());
    public virtual void Update() => currentState.Execute();
    public virtual void FixedUpdate() => currentState.FixedExecute();
}

