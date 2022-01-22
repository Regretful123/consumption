using System.Collections.Generic;
using UnityEngine;

public abstract class StateMachine
{
    State _state;

    public State GetState() => _state;

    public void SetState( State state )
    {
        _state?.OnExit();
        _state = state;
        _state.Init( this );
        _state?.OnEnter();
    }

    // Update is called once per frame
    void Update()
    {
        if( _state == null )
            return;
        _state.Execute();
    }
}
