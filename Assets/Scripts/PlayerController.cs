using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IDamagable
{
    const float airStrafe = 0.35f;
    const float landStrafe = 0.1f;
    [SerializeField] BoxCollider2D topCollider;
    [SerializeField, Range(0,1)] float crouchSpeed = 0.4f;
    // [SerializeField] Transform _displayMesh;
    [SerializeField] int startingHealth = 100;
    [SerializeField] Transform attackPoint;
    [SerializeField, Range(0.01f, 2f)] float attackRadius = 1f;

    public bool ground { get; private set; } = false;
    bool pauseControl = false;
    bool crouch = false;
    bool canStand = true;
    bool wasCrouching = false;
    bool freezeInput = false;

    [SerializeField] bool m_canDash = true;
    public bool canDash 
    {
        get => m_canDash;
        set 
        {
            if( m_canDash == value )
                return;
            
            m_canDash = value;
            if( m_canDash )
                _gameplayInputAction.Gameplay.Dash.Enable();
            else
                _gameplayInputAction.Gameplay.Dash.Disable();
        }
    }
    [SerializeField] bool m_canJump = true;
    public bool canJump
    {
        get => m_canJump;
        set
        {
            if( m_canJump == value )
                return;
            m_canJump = value;
            if( m_canJump )
                _gameplayInputAction.Gameplay.Jump.Enable();
            else
                _gameplayInputAction.Gameplay.Jump.Disable();
        }
    }

    [SerializeField] bool m_canAttack = true;
    public bool canAttack 
    {
        get => m_canAttack;
        set 
        {
            if( m_canAttack == value )
                return;
            m_canAttack = value;
            if( m_canAttack )
                _gameplayInputAction.Gameplay.Attack.Enable();
            else
                _gameplayInputAction.Gameplay.Attack.Disable();
        }
    }

    [Range(0, 100f)] public float speed = 5f;

    float _moveSmooth = 0.05f;

    [SerializeField, Range(0, 100f)] float jumpForce = 10f;

    public long experience { get; private set; } = 0;

    Health _health;
    public Health health { get => _health; }
    [SerializeField] int attackDamage = 0;

#region ceiling

    [SerializeField, Range(0.01f, 10f)] float ceilingRadius;
    [SerializeField] Transform ceilingDetection;

#endregion

#region ground
    [SerializeField, Range(0.01f, 5f)] float groundRadius;
    [SerializeField] Transform groundDetection;
    const int ignoreRaycastLayer = 2;

#endregion

#region Dash

    [SerializeField, Range(0.01f, 5f)] float dashDistance;
    [SerializeField] int playerLayer;
    [SerializeField, Range( 0.01f, 1f)] float dashDuration;

#endregion
    WaitForFixedUpdate endOfFrame = new WaitForFixedUpdate();
    public Action<PlayerController> onPlayerInteract;
    public Action<PlayerController> onPlayerLand;
    public Action<PlayerController> onPlayerCrouch;
    public Action<PlayerController> onPlayerStand;
    public Action<PlayerController, bool> onPlayerAttack;
    public Action<PlayerController, bool> onPlayerParry;

    public UnityEventInt onPlayerHealthChanged;
    public UnityEvent onPlayerDeath;

    Rigidbody2D _rb;
    Vector3 _velocity = Vector3.zero;
    Vector3 _targetVelocity = Vector3.zero;

    GamePlayInputAction _gameplayInputAction;
    InputAction _movement;
    bool hasStarted = false;
    bool hasDash = false;
    public GamePlayInputAction GetGamePlayInputAction() => _gameplayInputAction;

#region Unity event

    void Awake()
    {
        _health = new Health( startingHealth );
        _gameplayInputAction  = new GamePlayInputAction();
        _movement = _gameplayInputAction.Gameplay.Move;
    }

    void Start()
    {
        hasStarted = true;
        EnableInput();
        _rb = GetComponent<Rigidbody2D>();
        AttachHooks();
    }

    void OnEnable()
    {
        if( !hasStarted )
            return;
        EnableInput();
    }

    void OnDisable() => DisableInput();
    void OnDestroy() => DetachHooks();

    void FixedUpdate()
    {
        CheckCeiling();
        CheckGround();
        
        // if for whatever reason (Stun/bonk/unconscious) we need to be able to control this.
        if( ground || !pauseControl )
            Move();
    }

    // for debug stuff
    void OnDrawGizmosSelected()
    {
        if( groundDetection != null )
        {
            Gizmos.color = ground ? Color.green : Color.red;
            Gizmos.DrawWireSphere( groundDetection.position, groundRadius );
        }

        if( ceilingDetection != null )
        {
            Gizmos.color = canStand ? Color.green : Color.red;
            Gizmos.DrawWireSphere( ceilingDetection.position, ceilingRadius );
        }

        if( attackPoint != null )
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere( attackPoint.position, attackRadius );
        }
    }

    #endregion

#region private implementation

    void EnableInput()
    {
        _movement.Enable();
        if( canJump )
            _gameplayInputAction.Gameplay.Jump.Enable();
        if( canAttack )
            _gameplayInputAction.Gameplay.Attack.Enable();
        if( canDash )
            _gameplayInputAction.Gameplay.Dash.Enable();
        _gameplayInputAction.Gameplay.Interact.Enable();
        _gameplayInputAction.Gameplay.Explosion.Enable();
        _gameplayInputAction.Gameplay.Parry.Enable();
    }

    void DisableInput()
    {
        _movement.Disable();
        _gameplayInputAction.Gameplay.Jump.Disable();
        _gameplayInputAction.Gameplay.Attack.Disable();
        _gameplayInputAction.Gameplay.Dash.Disable();
        _gameplayInputAction.Gameplay.Interact.Disable();
        _gameplayInputAction.Gameplay.Explosion.Disable();
        _gameplayInputAction.Gameplay.Parry.Disable();
    }

    void AttachHooks()
    {
        _gameplayInputAction.Gameplay.Jump.performed += OnJump;
        _gameplayInputAction.Gameplay.Attack.started += OnAttack;
        _gameplayInputAction.Gameplay.Attack.canceled += OnAttack;
        _gameplayInputAction.Gameplay.Dash.performed += OnDash;
        _gameplayInputAction.Gameplay.Interact.started += OnInteract;
        _gameplayInputAction.Gameplay.Explosion.performed += OnExplosion;
        _gameplayInputAction.Gameplay.Parry.started += OnParry;
        _gameplayInputAction.Gameplay.Parry.canceled += OnParry;
        _health.onHealthDepleted += HandlePlayerDeath;
    }

    void DetachHooks()
    {
        _gameplayInputAction.Gameplay.Jump.performed -= OnJump;
        _gameplayInputAction.Gameplay.Attack.started -= OnAttack;
        _gameplayInputAction.Gameplay.Attack.canceled -= OnAttack;
        _gameplayInputAction.Gameplay.Dash.performed -= OnDash;
        _gameplayInputAction.Gameplay.Interact.started -= OnInteract;
        _gameplayInputAction.Gameplay.Explosion.performed -= OnExplosion;
        _gameplayInputAction.Gameplay.Parry.started -= OnParry;
        _gameplayInputAction.Gameplay.Parry.canceled -= OnParry;
        _health.onHealthDepleted -= HandlePlayerDeath;
    }

    int playerMask => ( 1 << playerLayer ) | ( 1 << ignoreRaycastLayer );

#endregion

#region Input System Callback

    void OnJump( InputAction.CallbackContext context ) => Jump();

    void OnAttack( InputAction.CallbackContext context ) => Melee( context );

    void OnDash( InputAction.CallbackContext context ) => Dash();

    void OnExplosion( InputAction.CallbackContext context )
    {
        // throw new NotImplementedException();
    }

    void OnInteract( InputAction.CallbackContext context ) => Interact();

    void OnParry( InputAction.CallbackContext context ) => Parry( context );

#endregion

#region Player Implementation

    void HandlePlayerDeath( Health health ) => onPlayerDeath?.Invoke();

    void CheckCeiling()
    {
        canStand = true;
        if( Physics2D.OverlapCircle( ceilingDetection.position, ceilingRadius, ~playerMask ))
        {
            canStand = false;
            crouch = true;
        }
    }

    void CheckGround()
    { 
        bool wasGrounded = ground;
        ground = false;
        Collider2D[] cols = Physics2D.OverlapCircleAll( groundDetection.position, groundRadius, ~playerMask );
        foreach( var col in cols )
        {
            if( col.gameObject != gameObject )
            {
                ground = true;
                if( wasGrounded )
                {
                    hasDash = false;// reset dash
                    _moveSmooth = landStrafe;
                    onPlayerLand?.Invoke( this );
                }
                return;
            }
        }

        if( !wasGrounded )
            _moveSmooth = airStrafe;
    }

    void CheckCrouch(Vector2 input)
    {
        if( input.y < 0f && !crouch )
        {
            crouch = true;
            if( !wasCrouching )
            {
                wasCrouching = true;
                if( topCollider != null )
                    topCollider.enabled = false;
                onPlayerCrouch?.Invoke(this);
            }
            _rb.AddForce( Physics2D.gravity * 0.5f, ForceMode2D.Impulse );
        }
        else if( input.y >= 0f && crouch && canStand )
        {
            crouch = false;
            if( wasCrouching )
            {
                wasCrouching = false;
                if( topCollider != null )
                    topCollider.enabled = true;
                onPlayerStand?.Invoke(this);
            }
        }
    }

    void Crouch()
    {
        if( crouch )
        {
            if( !wasCrouching )
            {
                wasCrouching = true;
                onPlayerCrouch?.Invoke( this );
            }
        }
    }

    void Dash()
    {
        // I've imagine teh follow behaviour
        // when pressed, an animation will play to show sliding across something.
        // rigidbody will be assigned every frame for the slide.
        // interpolate between two points.
        if( !hasDash )
        {
            hasDash = true;
            StartCoroutine( Dashing() );
        }
    }

    IEnumerator Dashing()
    {
        // initial variables
        Vector2 orgPos = transform.position;
        float _t = 0;
        float dir = transform.localScale.x < 0 ? -1 : 1;

        // check and see if there's a wall in front of the player, stop there instead of going through
        RaycastHit2D hit = Physics2D.Raycast( orgPos, transform.right * dir, dashDistance, ~playerMask );
        Vector3 target = orgPos + (Vector2)transform.right * dashDistance;
        if( hit )
            target = (Vector2)hit.point - ( (Vector2)transform.right * dir * ( transform.localScale.x / 2 ) ); 
        
        // freeze input while we're animating the dash.
        freezeInput = true;
        while( _t < 1 )
        {
            _t += Time.fixedDeltaTime / dashDuration;
            _rb.position = Vector3.Lerp( orgPos, target, _t );
            yield return endOfFrame;
        }
        freezeInput = false;
    }

    void Slide()
    {
        // in here we'll somehow simulate the sliding animation... Should be interesting!
    }

    void Interact() => onPlayerInteract?.Invoke( this );

    void Melee( InputAction.CallbackContext context ) => onPlayerAttack?.Invoke( this, context.ReadValue<float>() > 0.5f );

    void Parry( InputAction.CallbackContext context ) => onPlayerParry?.Invoke( this, context.ReadValue<float>() > 0.5f );

    void Move()
    {
        Vector2 movement = _movement.ReadValue<Vector2>();
        float move = movement.x * speed;
        CheckCrouch( movement );

        if( crouch )
            move *= crouchSpeed;

        _targetVelocity = _rb.velocity;
        _targetVelocity.x = move;
        _targetVelocity.y = Mathf.Max( Physics2D.gravity.y, _targetVelocity.y );
        _rb.velocity = Vector3.SmoothDamp( _rb.velocity, _targetVelocity, ref _velocity, _moveSmooth);
     
        Vector3 _scale = transform.localScale;
        if( ( _scale.x > 0 && move < 0 ) || ( _scale.x < 0 && move > 0 ) )
        {
            _scale.x *= -1;
            transform.localScale = _scale;
        }
    }

    void Jump()
    {
        if( ground && !crouch )
        {
            RaycastHit2D hit = Physics2D.CircleCast( groundDetection.position, groundRadius, -transform.up, Mathf.Infinity, ~playerMask ); 
            ground = false;
            Vector2 hitPointNormal = ( (Vector2)groundDetection.position - hit.point ).normalized;
            Vector2 jumpDir = ( Vector2.up + hitPointNormal ).normalized;
            _rb.AddForce( jumpDir * jumpForce, ForceMode2D.Impulse );
        }
    }

    public void OnHurt( int damages ) => _health.OnHurt( damages );

    public void OnHeal( int heal ) => _health.OnHeal( heal );

    Collider2D[] cols = new Collider2D[5];
    public void ApplyDamage()
    {
        // received animator to "Damage" attacks.
        int count = Physics2D.OverlapCircleNonAlloc( attackPoint.position, attackRadius, cols, ~playerMask );
        for( int i = 0; i<count; ++i )
        {
            if( cols[i].TryGetComponent<IDamagable>(out IDamagable _target ))
                _target.OnHurt( attackDamage );
        }
    }

    public Vector3 velocity => _rb.velocity;

    #endregion
}
