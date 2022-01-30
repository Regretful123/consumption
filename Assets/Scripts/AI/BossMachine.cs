using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMachine : StateMachine, IDamagable
{
    public enum BossEvent : byte
    {
        Intro = 0,
        Idle,
        Attack,
        Recover,
        Hurt,
        Roar,
        Dead
    }

    Health _health;
    public Health health => throw new System.NotImplementedException();
    public float idleTime = 1f;
    public float attackDuration = 2f;
    // public float stunDuration = 2f;
    public float roarDuration = 1f;
    public int damages = 10;
    public int initialHealth = 10;
    public BossEvent currentEvent = BossEvent.Intro;
    public BossPieceController[] pieces;
    [SerializeField] Animation m_anim;
    public Action<BossEvent> OnBossChanged;
    
    internal float introDuration = 1f;
    internal float recoverDuration = 1f;
    internal float dyingAnimTime = 1f;

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

    public void OnHurt(int damages)
    {
        // SetState( new Hurt(this));
    }

    void Start()
    {
        _health = new Health( initialHealth );
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
            dyingAnimTime = dyingClip.length;

        idleTime = Mathf.Max( pieceIdleClip.length, idleClip.length );
        attackDuration = Mathf.Max( pieceAttackClip.length, attackClip.length );
        recoverDuration = Mathf.Max( pieceRecoveryClip.length, recoverClip.length );

        ToIntro();
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

    void ToIntro() => SetState( new Intro(this), BossEvent.Intro);
    internal void ToIdle() => SetState( new Idle(this), BossEvent.Idle);
    internal void ToAttack() => SetState( new Idle(this), BossEvent.Attack);
    internal void ToRecover() => SetState( new Recover(this), BossEvent.Recover );
    internal void ToRoar() => SetState( new Idle(this), BossEvent.Roar);
    internal void OnDeath() => SetState( new Dying(this), BossEvent.Dead );

    class Idle : BossState
    {
        public Idle(BossMachine stateMachine) : base(stateMachine) { }

        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.idleTime );
            float percent = UnityEngine.Random.Range(0f,1f);
            if( percent > 0.5f )
                stateMachine.ToRoar();
            else
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
            stateMachine.SetHitboxes( true );
            yield return new WaitForSeconds( stateMachine.attackDuration );
            stateMachine.SetHitboxes( false );
            stateMachine.ToRecover();
        }
    }

    class Recover : BossState
    {
        public Recover(BossMachine stateMachine ) : base( stateMachine) { }
        public override IEnumerator Init()
        {
            yield return new WaitForSeconds( stateMachine.recoverDuration );
            stateMachine.ToIdle();
        }
    }

    // class Hurt : BossState
    // {
    //     public Hurt( BossMachine stateMachine ) : base(stateMachine) { }

    //     // guess we'll play some animations here?
    //     public override IEnumerator Init()
    //     {
    //         yield return new WaitForSeconds( stateMachine. );
    //         stateMachine.ToIdle();
    //     }
    // }
    
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