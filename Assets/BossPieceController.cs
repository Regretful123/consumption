using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPieceController : Damagable
{
    BossMachine m_bossMachine;
    [SerializeField] int damages = 10;
    [SerializeField] Animation m_anim;
    [SerializeField] Collider2D attackCollider;

    public AnimationClip introClip;
    public AnimationClip idleClip;
    public AnimationClip attackClip;
    public AnimationClip recoverClip;
    public AnimationClip hurtClip;
    public AnimationClip dyingClip;

    override public void Start()
    {
        base.Start();
        health.onHealthDepleted += HandlePieceDeath;
    }

    private void HandlePieceDeath(Health health)
    {
        Destroy(gameObject);
    }

    public void Init( BossMachine machine )
    {
        m_bossMachine = machine;
        if( m_bossMachine != null )
            m_bossMachine.OnBossChanged += HandleBossStateChanged;
    }

    void OnDestroy()
    {
        if( m_bossMachine != null )
            m_bossMachine.OnBossChanged -= HandleBossStateChanged;
        health.onHealthDepleted -= HandlePieceDeath;
    }

    void PlayAnimation( AnimationClip clip )
    {
        if( m_anim == null || m_anim.clip == clip )
            return;
        m_anim.clip = clip;
        m_anim.Play();
    }

    Collider2D[] cols = new Collider2D[5];
    void ApplyDamage()
    {
        if( attackCollider == null )
            return;

        Vector2 colPos = attackCollider.transform.position;
        int count = 0;
        switch( attackCollider )
        {
            case BoxCollider2D box : 
                count = Physics2D.OverlapBoxNonAlloc( colPos + box.offset, box.size * Vector2.one * 0.2f, 0, cols, m_bossMachine.damageTo ); 
                break;
            case CircleCollider2D circle : 
                count = Physics2D.OverlapCircleNonAlloc( colPos + circle.offset, circle.radius + 0.2f, cols, m_bossMachine.damageTo );
                break;
        }

        for( int i = 0; i<count; ++ i )
        {
            if( cols[i].TryGetComponent<IDamagable>(out IDamagable _target ))
                _target.OnHurt( damages );
        }
    }

    private void HandleBossStateChanged(BossMachine.BossEvent stateEvent)
    {
        switch( stateEvent )
        {
            case BossMachine.BossEvent.Intro : PlayAnimation( introClip ); break;
            case BossMachine.BossEvent.Idle : PlayAnimation( idleClip ); break;
            case BossMachine.BossEvent.Attack : PlayAnimation( attackClip ); break;
            case BossMachine.BossEvent.Recover : PlayAnimation( recoverClip ); break;
            case BossMachine.BossEvent.Dead : PlayAnimation( dyingClip ); break;
            case BossMachine.BossEvent.Hurt : PlayAnimation( hurtClip ); break;
            case BossMachine.BossEvent.Roar : PlayAnimation( idleClip ); break;
        }
    }

    override public void OnHurt( int damages )
    {
        base.OnHurt(damages);
        Debug.Log($"tentacle receiving {damages} damages! | {health.currentHealth} remaining");
        m_bossMachine.OnHurt( damages );    // wonder if I need to accumulate enough points for this? or nah? Who knows?
    }

    public void SetTriggerStatus(bool isEnable)
    {
        if( attackCollider != null )
        {
            if( isEnable )
                ApplyDamage();
            attackCollider.enabled = isEnable;
        }
    }
}
