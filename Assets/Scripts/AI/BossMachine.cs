using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class BossMachine : StateMachine, IDamagable
{
    public enum BossEvent : byte
    {
        Intro = 0,
        Idle,
        Attack,
        Pause,
        Recover,
        Hurt,
        Roar,
        Dead
    }

    Collider2D[] cols = new Collider2D[5];
    Health _health;
    public Health health => throw new System.NotImplementedException();
    public int damages = 10;
    public int initialHealth = 10;
    public BossEvent currentEvent = BossEvent.Intro;
    public BossPieceController[] pieces;
    [SerializeField] Animation m_anim;
    public Collider2D headCollider;
    public Action<BossEvent> OnBossChanged;

    public UnityEvent OnBossDeath;

    
    internal float introDuration = 1f;
    internal float recoverDuration = 1f;
    internal float dyingDuration = 1f;
    internal float idleDuration = 1f;
    internal float attackDuration = 2f;
    internal float roarDuration = 1f;
    internal float attackPauseDelay = 3f;
    internal float headAttackAnimDelay = 0f;
    
    public LayerMask damageTo;
    [SerializeField] AnimationClip introClip;
    [SerializeField] AnimationClip idleClip;
    [SerializeField] AnimationClip attackClip;
    [SerializeField] AnimationClip recoverClip;
    [SerializeField] AnimationClip roarClip;
    [SerializeField] AnimationClip dyingClip;

    [Header("Pieces information")]
    [SerializeField] AnimationClip pieceIntroClip;
    [SerializeField] AnimationClip pieceIdleClip;
    [SerializeField] AnimationClip pieceAttackClip;
    [SerializeField] AnimationClip pieceRecoveryClip;
    [SerializeField] AnimationClip pieceDyingClip;

    public void OnHeal(int heal) { }

    public void OnHurt(int damages) => _health.OnHurt( damages );

    void Start()
    {
        _health = new Health( initialHealth );
        _health.onHealthDepleted += HandleBossDeath;

        // disable collider on start.
        foreach( var col in pieces )
        {
            col.Init( this );
            _health.OnHeal( col.initialHealth );
            col?.SetTriggerStatus( false );
        }

        // get length of the clip duration for animations, otherwise default to 1 sec
        if( idleClip != null )
            introDuration = idleClip.length;
        if( dyingClip != null )
            dyingDuration = dyingClip.length;

        introDuration = Mathf.Max( introClip.length, pieceIntroClip.length );
        idleDuration = Mathf.Max( pieceIdleClip.length, idleClip.length );
        attackDuration = Mathf.Max( pieceAttackClip.length, attackClip.length );
        recoverDuration = Mathf.Max( pieceRecoveryClip.length, recoverClip.length );
        OnBossChanged += HandleCurrentState;
        ToIntro();
    }

    void OnDestroy()
    {
        OnBossChanged -= HandleCurrentState;
        _health.onHealthDepleted -= HandleBossDeath;
    }

    void OnDrawGizmosSelected()
    {
        if( headCollider != null )
        {
            Vector2 headPos = headCollider.transform.position; 
            Gizmos.color = Color.red;
            switch( headCollider )
            {
                case BoxCollider2D box : Gizmos.DrawWireCube( headPos + box.offset, box.size + Vector2.one * 0.1f ); break;
                case CircleCollider2D circle : Gizmos.DrawWireSphere( headPos + circle.offset, circle.radius + 0.5f ); break;
            }
        }
    }

    private void HandleBossDeath(Health obj)
    {
        OnBossDeath?.Invoke();
        foreach( var col in pieces )
            if( col != null )
                Destroy( col );
        Destroy(gameObject);

        // according to Norgoroth, send this scene to the credit scene. 
        SceneManager.LoadScene(0); // TODO change this to credit when we have the scene!
    }

    void PlayAnimation(AnimationClip clip )
    {
        if( m_anim == null || m_anim.clip == clip )
            return;
        m_anim.clip = clip;
        m_anim.Play();
    }

    void HandleCurrentState( BossEvent evt )
    {
        switch( evt )
        {
            case BossEvent.Intro : PlayAnimation(introClip); break;
            case BossEvent.Idle : PlayAnimation(idleClip); break;
            case BossEvent.Attack : PlayAnimation(attackClip); break;
            case BossEvent.Pause : break; // quite literally taking a pause here...
            case BossEvent.Recover : PlayAnimation(recoverClip); headCollider.enabled = false; break;
            case BossEvent.Roar : PlayAnimation(roarClip); break;
            case BossEvent.Hurt : PlayAnimation(idleClip); break;
            case BossEvent.Dead : PlayAnimation(dyingClip);  break;
        }
    }

    void SetState( State newState, BossEvent evt )
    {
        SetState( newState );
        currentEvent = evt;
        OnBossChanged?.Invoke(evt);
    }

    internal void SetHitboxes( bool isEnable )
    {
        foreach( var col in pieces )
            col?.SetTriggerStatus( isEnable );
    }

    internal void ApplyDamage()
    {
        if( headCollider == null )
            return;
        
        int count = 0;
        Vector2 headPos = headCollider.transform.position; 
        switch( headCollider )
        {
            case BoxCollider2D box : 
                count = Physics2D.OverlapBoxNonAlloc( headPos + box.offset, box.size + Vector2.one * 0.1f, 0, cols, damageTo ); 
                break;
            case CircleCollider2D circle : 
                // damageTo
                count = Physics2D.OverlapCircleNonAlloc( headPos + circle.offset, circle.radius + 0.5f, cols, damageTo ); 
                break;
        }

        for(int i = 0; i<count; ++i )
        {
            if( cols[i].TryGetComponent<IDamagable>( out IDamagable _target ))
                _target.OnHurt( damages );
        }
    }

    void ToIntro() => SetState( new Intro(this), BossEvent.Intro);
    internal void ToIdle() => SetState( new Idle(this), BossEvent.Idle);
    internal void ToAttack() => SetState( new Attack(this), BossEvent.Attack);
    internal void ToPause() => SetState( new Pause(this), BossEvent.Pause );
    internal void ToRecover() => SetState( new Recover(this), BossEvent.Recover );
    internal void ToRoar() => SetState( new Roar(this), BossEvent.Roar);
    internal void OnDeath() => SetState( new Dying(this), BossEvent.Dead );

    class Idle : BossState
    {
        public Idle(BossMachine stateMachine) : base(stateMachine) { }

        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.idleDuration );
            // float percent = UnityEngine.Random.Range(0f,1f);
            // if( percent > 0.5f )
            //     stateMachine.ToRoar();
            // else
                stateMachine.ToAttack();
        }
    }

    class Roar : BossState
    {
        public Roar(BossMachine stateMachine ) : base(stateMachine ) { }

        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.roarDuration );
            stateMachine.ToIdle();
        }
    }

    class Attack : BossState
    {
        public Attack(BossMachine stateMachine ) : base(stateMachine) { }

        public override IEnumerator Init()
        {
            if( stateMachine.headAttackAnimDelay > stateMachine.attackDuration )
            {
                yield return new WaitForSeconds( stateMachine.attackDuration );
                stateMachine.SetHitboxes( true );
                yield return new WaitForSeconds( stateMachine.headAttackAnimDelay - stateMachine.attackDuration );
                stateMachine.ApplyDamage();
                stateMachine.headCollider.enabled = true;
            }
            else
            {
                yield return new WaitForSeconds( stateMachine.headAttackAnimDelay );
                stateMachine.ApplyDamage();
                stateMachine.headCollider.enabled = true;
                yield return new WaitForSeconds( stateMachine.attackDuration - stateMachine.headAttackAnimDelay );
                stateMachine.SetHitboxes( true );
            }
            stateMachine.ToPause();
        }
    }

    class Pause : BossState
    {
        public Pause(BossMachine stateMachine) : base(stateMachine){ }

        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.attackPauseDelay );
            stateMachine.SetHitboxes( false );
            stateMachine.ToRecover();
        }
    }

    class Recover : BossState
    {
        public Recover(BossMachine stateMachine ) : base( stateMachine) { }
        public override IEnumerator Init()
        {
            stateMachine.headCollider.enabled = false;
            yield return new WaitForSeconds( stateMachine.recoverDuration );
            stateMachine.ToIdle();
        }
    }
    
    class Intro : BossState
    {
        public Intro(BossMachine stateMachine ) : base(stateMachine) { }

        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.introDuration );
            stateMachine.ToIdle();
        }
    }

    class Dying : BossState
    {
        public Dying(BossMachine stateMachine ) : base(stateMachine) { }

    }
}

public class BossState : State
{
    protected new BossMachine stateMachine;

    public BossState(BossMachine stateMachine) : base(stateMachine) => this.stateMachine = stateMachine;
}