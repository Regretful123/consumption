using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : State
{
    [SerializeField] float dmg = 100;

    public AttackState(StateMachine stateMachine) : base(stateMachine) { }

    public override void Execute()
    {
        throw new System.NotImplementedException();
    }
    // [SerializeField] Collider2D 

}
