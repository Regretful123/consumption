using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator)), DisallowMultipleComponent]
public class CharacterAnimController : MonoBehaviour
{
    [SerializeField] Animator _anim;
    [SerializeField] PlayerController _controller;

    int horizontalHash = Animator.StringToHash("horizontal");
    int verticalHash = Animator.StringToHash("vertical");
    int parryHash = Animator.StringToHash("isParry");
    int groundHash = Animator.StringToHash("isGround");
    int attackHash = Animator.StringToHash("isAttacking");


    // Start is called before the first frame update
    void Start()
    {
        if( _anim == null )
            _anim = GetComponent<Animator>();

        if( _controller == null )
        {
            Debug.LogError("Player controller is not assigned!", gameObject );
            this.enabled = false;
            return;
        }
        _controller.onPlayerAttack += HandlePlayerAttack;
        _controller.onPlayerLand += HandlePlayerLand;
        _controller.onPlayerHealthChanged.AddListener( HandlePlayerHealth );
        // _controller.onPlayerCrouch += HandlePlayerCrouch;
    }

    private void HandlePlayerHealth( int newHealth )
    {
        // maybe a different animation could be played here?
    }

    private void HandlePlayerAttack(PlayerController obj)
    {
        _anim.SetBool( attackHash, true );
    }

    private void HandlePlayerLand(PlayerController obj)
    {
        throw new NotImplementedException();
    }
}
