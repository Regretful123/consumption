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
        
        _controller.onPlayerAttack += HandleAttack;
        _controller.onPlayerParry += HandleParry;
        _controller.onPlayerLand += HandleLand;

        // _controller.onPlayerHealthChanged.AddListener( HandlePlayerHealth );
        // _controller.onPlayerCrouch += HandlePlayerCrouch;
    }

    void OnDestroy()
    {
        if( _controller != null )
        {
            _controller.onPlayerAttack -= HandleAttack;
            _controller.onPlayerParry -= HandleParry;
            _controller.onPlayerLand -= HandleLand;
        }
    }

    private void HandleParry(PlayerController obj)
    {
        _anim.SetBool( parryHash, true );
    }

    private void HandlePlayerHealth( int newHealth )
    {
        // maybe a different animation could be played here?
    }

    void HandleAttack( PlayerController obj ) => _anim.SetBool( attackHash, true );

    void HandleLand(PlayerController obj) => _anim.SetBool( groundHash, true );

    // private void HandlePlayerCrouch(PlayerController obj)
    // {
    //     _anim.SetBool();
    // }
}
