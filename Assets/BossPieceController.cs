using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPieceController : MonoBehaviour, IDamagable
{
    Health _health; 
    BossMachine m_bossMachine;
    public Health health => _health;
    public int initialHealth = 100;
    [SerializeField] Animation m_anim;
    [SerializeField] Collider2D attackCollider;

    public AnimationClip introClip;
    public AnimationClip idleClip;
    public AnimationClip attackClip;
    public AnimationClip recoverClip;
    public AnimationClip hurtClip;
    public AnimationClip dyingClip;

    void Start()
    {
        _health = new Health( initialHealth );
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
    }

    void PlayAnimation( AnimationClip clip )
    {
        if( m_anim == null )
            return;
        m_anim.clip = clip;
        m_anim.Play();
    }

    private void HandleBossStateChanged(BossMachine.BossEvent stateEvent)
    {
        switch( stateEvent )
        {
            case BossMachine.BossEvent.Idle : PlayAnimation( idleClip ); break;
            case BossMachine.BossEvent.Attack : PlayAnimation( attackClip ); break;
            case BossMachine.BossEvent.Recover : PlayAnimation( recoverClip ); break;
            case BossMachine.BossEvent.Dead : PlayAnimation( dyingClip ); break;
            case BossMachine.BossEvent.Hurt : PlayAnimation( hurtClip ); break;
            case BossMachine.BossEvent.Roar : PlayAnimation( idleClip ); break;
        }
    }

    public void OnHeal(int heal) { }

    public void OnHurt(int damages)
    {
        m_bossMachine.OnHurt( damages );    // wonder if I need to accumulate enough points for this? or nah? Who knows?
    }

    public void SetTriggerStatus(bool isEnable)
    {
        if( attackCollider != null )
            attackCollider.enabled = isEnable;
    }

}
