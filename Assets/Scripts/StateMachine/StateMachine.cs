using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine : MonoBehaviour
{
    [SerializeField] protected State currentState;  // tells me which state I am currently running in
    public State GetState() => currentState; // returns the property of state 
    public virtual void SetState( State newState ) 
    {
        Debug.Log($"[{newState.GetType().Name}] has been set!", gameObject );
        StartCoroutine((currentState = newState).Init());
    }
    public virtual void Update() => currentState.Execute();
    public virtual void FixedUpdate() => currentState.FixedExecute();
}

