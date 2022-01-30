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
    int parryLayerId = 0;
    int groundHash = Animator.StringToHash("isGround");
    int attackHash = Animator.StringToHash("isAttacking");
    int initialParryHash = Animator.StringToHash("ParryRelease");
    int attackLayerId = 0;

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
        parryLayerId = _anim.GetLayerIndex("Parry");
        attackLayerId = _anim.GetLayerIndex("Attack");
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

    void Update()
    {
        Vector3 _vel = _controller.velocity;
        _anim.SetBool( groundHash, _controller.ground );
        _anim.SetFloat( horizontalHash, Mathf.Abs( _vel.x ) );
        _anim.SetFloat( verticalHash, _vel.y );
    }

    void OnAttackDamage() => _controller.ApplyDamage();

    private void HandleParry(PlayerController obj, bool isDefending )
    {
        if( isDefending )
            _anim.SetLayerWeight( parryLayerId, 1f );
        _anim.SetBool( parryHash, isDefending  );
        _anim.Play( initialParryHash, parryLayerId );
    }

    private void HandlePlayerHealth( int newHealth )
    {
        // maybe a different animation could be played here?
    }

    void HandleAttack( PlayerController obj, bool isAttacking )
    {
        _anim.SetLayerWeight( attackLayerId, 1f );
        _anim.SetBool( attackHash, isAttacking );
    } 

    void HandleLand(PlayerController obj) => _anim.SetBool( groundHash, true );

    // private void HandlePlayerCrouch(PlayerController obj)
    // {
    //     _anim.SetBool();
    // }
}
