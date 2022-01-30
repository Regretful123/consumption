using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
public class AIMachine : StateMachine, IDamagable
{

#region private variable

    // AI desire direction
    Vector3 targetVelocity = Vector3.zero;     // current rb velocity movement
    Vector3 velocity = Vector3.zero; // local scale to apply (flip/switch AI)
    Vector3 scale = Vector3.one; // local scale ref 
    Rigidbody2D _rb; // reference to rigidbody2D
    bool ground = false; // is ground

    [SerializeField, Tooltip("Target of interest")] 
    Transform m_target;  // assign a transform here to make the AI chase the object.

    [SerializeField, Tooltip("Transformation of where to check the ground from")] 
    Transform _groundPoint; // how the ai detects ground.
    Collider2D[] cols = new Collider2D[5];  // array to keep track of ground check (avoid GC)

#endregion

#region public variable

    public Collider2D attackCollider;
    
    [Range(0.001f, 10)]
    public float groundRadius = 1f;
    public LayerMask groundMask;

    Health _health;
    public Health health => _health;

    [SerializeField] List<Transform> waypoints = new List<Transform>();
    int index = 0;

    public float viewDistance = 10f;
    public float visionAngle = 45f;
    public int initialHealth = 100;

    public float speed = 1f;
    public float chaseSpeed = 5f;
    public float moveSmooth = 0.2f;
    public int damage = 10;
    public float fleeSpeed = 10f;
    [Range(0,1)]
    public float fleeChance = 0.2f;
    public float fleeRange = 10f;
    public bool canStun = true;
    public float stunDuration = 0f;
    public float waitTime = 0f;

    [Range(0,1)]
    public float scareFactor = 0.5f;

    // public float canJump?
    [Range(0, 100)]
    public float attackRate = 0f;
    [Range(0, 100)]
    public float attackRange = 2f;
    public Transform wallPoint;
    public float wallRadius; 
    public LayerMask wallMask;
    bool isWall = false;

#endregion

#region properties

    public Rigidbody2D rb => _rb;
    public Transform target => m_target;
    public Transform groundPoint => _groundPoint;
    public bool IsGround => ground;
    public bool WallExist => isWall;

#endregion

#region Unity Events

    void OnDrawGizmosSelected()
    {
        Vector3 groundPos = ( groundPoint ?? this.transform ).position;

        Gizmos.color = ground ? Color.green : Color.red;
        Gizmos.DrawWireSphere( groundPos, groundRadius );

        if( target != null )
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine( transform.position, target.position );
        }

        if( wallPoint != null )
        {
            Gizmos.color = isWall ? Color.green : Color.red;
            Gizmos.DrawWireSphere( wallPoint.position, wallRadius );
        }

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere( transform.position, viewDistance );
    }

    void Awake()
    {
        _groundPoint ??= this.transform;
        _health = new Health(initialHealth);
        _health.onHealthDepleted += HandleAIDeath;
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        scale = transform.localScale;
        ToMove();
    }

    public override void Update() => currentState?.Execute();

    public override void FixedUpdate()
    {
        if( m_target != null && !m_target.gameObject.activeInHierarchy )
            UnassignTarget();
        GroundCheck();
        currentState?.FixedExecute();
    }

    void OnDestroy()
    {
        if( _health != null )
        {
            _health.onHealthDepleted -= HandleAIDeath;
        }
    }

#endregion

#region private implementation

    internal void ToStun() => SetState( new Stun( this ));
    internal void ToFlee() => SetState( new Flee( this ));
    internal void ToAttack() => SetState( new Attack( this ));
    internal void ToChase() => SetState( new Chase( this ));
    internal void ToMove() => SetState( new Move( this ));
    void GroundCheck()
    {
        ground = false;
        int count = Physics2D.OverlapCircleNonAlloc( groundPoint.position, groundRadius, cols, groundMask );
        for( int i = 0; i<count; ++ i )
        {
            if( cols[i].gameObject.Equals( this.gameObject ))
                continue;
            ground = true;
            break;
        }
    }
    
    void HandleAIDeath( Health health )
    {
        // TODO What should happen if the AI is dead?
        Destroy( this.gameObject ); 
    }

#endregion

#region public interface

    public void SetTarget( Transform target ) => m_target = target;
    public void UnassignTarget() => m_target = null;
    public void SetTarget( GameObject target ) => m_target = target.transform;

    public Transform GetNextWaypoint()
    {
        if( ++index >= waypoints.Count )
            index = 0;
        return waypoints[index];   
    }

    public float distanceToTarget => target != null ? Vector3.Distance( transform.position, target.position ) : Mathf.Infinity;

    public bool targetVisible => target != null && viewDistance > distanceToTarget;

    public void Move( float directionForce )
    {
        targetVelocity = _rb.velocity;
        targetVelocity.x = directionForce * ( scale.x < 0 ? -1 : 1 );
        targetVelocity.y = Mathf.Max( Physics2D.gravity.y, targetVelocity.y );
        _rb.velocity = Vector3.SmoothDamp( _rb.velocity, targetVelocity, ref velocity, moveSmooth );
    }

    public void SwitchSide()
    {
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void OnHurt(int damages) 
    {
        _health.OnHurt( damages );
        SetState( new Hurt(this));
    }

    // public void OnHeal(int heal) => SetState( new Move(this));
    public void OnHeal(int heal) { }

    public void CheckWall()
    {
        isWall = false;
        if( wallPoint == null ) 
            return;
        int count = Physics2D.OverlapCircleNonAlloc( wallPoint.position, wallRadius, cols, wallMask );     
        for( int i = 0; i<count; ++i )
        {
            if( cols[i].gameObject != gameObject )
            {
                isWall = true;
                break;
            }
        }   
    }

    #endregion
}

public abstract class AIState : State
{
    public new AIMachine stateMachine;
    protected AIState(AIMachine stateMachine) : base(stateMachine) => this.stateMachine = stateMachine;
}

public class Stun : AIState
{
    public Stun(AIMachine stateMachine) : base(stateMachine) { }
    public override IEnumerator Init()
    {
        yield return new WaitForSeconds(stateMachine.stunDuration);
        stateMachine.ToMove();
    }
}

public class Flee : AIState
{
    Vector3 velocity = Vector3.zero;
    Vector3 orgPos = Vector3.zero;
    int direction = 0;
    public Flee(AIMachine stateMachine) : base(stateMachine) 
    { 
        orgPos = stateMachine.transform.position;
        if( stateMachine.target != null )
        {
            direction = stateMachine.transform.position.x < stateMachine.target.position.x ? -1 : 1;
        }
        else
        {
            direction = stateMachine.transform.position.x < 0 ? 1 : -1;
        }
    }

    public override void FixedExecute()
    {
        bool inZone = Vector3.Distance( stateMachine.transform.position, orgPos ) >= stateMachine.fleeRange; 
        if( inZone )
            stateMachine.ToMove();
    }
}

public class Attack : AIState
{
    public Attack(AIMachine stateMachine) : base(stateMachine) { }

    public override IEnumerator Init()
    {
        bool repeat = false;
        do
        {    
            stateMachine.attackCollider.enabled = true;
            yield return new WaitForSeconds( stateMachine.attackRate );
            stateMachine.attackCollider.enabled = false;
            if( stateMachine.attackRange >= stateMachine.distanceToTarget )
                repeat = false;
        } 
        while( repeat );
        stateMachine.ToChase();
    }
}

public class Hurt : AIState
{
    public Hurt(AIMachine stateMachine ) : base(stateMachine) { }

    bool ShouldFlee() => stateMachine.health.currentHealth <= stateMachine.health.maxHealth * stateMachine.scareFactor 
                        && Random.Range(0f,1f) <= stateMachine.fleeChance; 

    public override IEnumerator Init()
    {
        if( stateMachine.canStun )
        {
            stateMachine.ToStun();
            yield break;
        }
        else if( ShouldFlee() )
        {
            stateMachine.ToFlee();
            yield break;
        }

        if( stateMachine.target != null )
        {
            float distance = Vector3.Distance( stateMachine.transform.position, stateMachine.target.position );
            if( distance <= stateMachine.attackRange )
            {
                stateMachine.ToAttack();
                yield break;
            }
            else if( distance <= stateMachine.viewDistance )
            {
                stateMachine.ToChase();
                yield break;
            }
        }
        stateMachine.ToMove();
        yield break;
    }
}

public class Chase : AIState
{
    float speed = 0f;
    public Chase(AIMachine stateMachine) : base(stateMachine) 
    {
        speed = stateMachine.chaseSpeed;
        if( stateMachine.target == null )
            stateMachine.ToMove();
    }

    public override void FixedExecute()
    {
        if( stateMachine.target == null )
        {
            stateMachine.ToMove(); // would like to implement the lost behavior here.
            return;
        }

        float dist = Vector3.Distance( stateMachine.transform.position, stateMachine.target.position );
        if( dist < stateMachine.attackRange )
        {
            stateMachine.ToAttack();
            return;
        }
        else if( dist > stateMachine.viewDistance )
        {
            stateMachine.ToMove();
            return;
        }
        
        int newDirection = stateMachine.transform.position.x > stateMachine.target.position.x ? -1 : 1;
        if( ( newDirection < 0 && stateMachine.transform.localScale.x > 0 ) ^ ( newDirection > 0 && stateMachine.transform.localScale.x < 0 ) )
            stateMachine.SwitchSide();

        stateMachine.Move( speed );
    }
}

public class Move : AIState
{
    float speed = 0f;
    bool wasGround = true;

    public override IEnumerator Init()
    {
        speed = stateMachine.speed;
        if( stateMachine.targetVisible )
            stateMachine.ToChase();
        yield break;
    }

    public override void FixedExecute()
    {
        stateMachine.CheckWall();
        if( stateMachine.WallExist )
            stateMachine.SwitchSide();

        if( !stateMachine.IsGround && wasGround )
        {
            stateMachine.SwitchSide();
            wasGround = false;
        }
        else if ( stateMachine.IsGround && !wasGround )
            wasGround = true;

        stateMachine.Move(speed);

        if( stateMachine.targetVisible )
            stateMachine.ToChase();
    }

    public Move(AIMachine stateMachine) : base(stateMachine) { }
}