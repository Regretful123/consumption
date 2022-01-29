using System;
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

    bool ground = false;
    bool pauseControl = false;
    bool crouch = false;
    bool canStand = true;
    bool wasCrouching = false;
    bool freezeInput = false;

    [Range(0, 100f)] public float speed = 5f;

    float _moveSmooth = 0.05f;

    [SerializeField, Range(0, 100f)] float jumpForce = 10f;

    public long experience { get; private set; } = 0;

    Health _health;
    public Health health { get => _health; }

    [SerializeField, Range(0.01f, 10f)] float ceilingRadius;
    [SerializeField, Range(0.01f, 5f)] float groundRadius;
    [SerializeField] Transform ceilingDetection;
    [SerializeField] Transform groundDetection;
    [SerializeField] LayerMask groundMask;

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
    }

    #endregion

#region private implementation

    void EnableInput()
    {
        _movement.Enable();
        _gameplayInputAction.Gameplay.Jump.Enable();
        _gameplayInputAction.Gameplay.Attack.Enable();
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
        // if( crouch )
        //     return;
            
        canStand = true;
        if( Physics2D.OverlapCircle( ceilingDetection.position, ceilingRadius, groundMask))
        {
            canStand = false;
            crouch = true;
        }
    }


    void CheckGround()
    { 
        bool wasGrounded = ground;
        ground = false;

        Collider2D[] cols = Physics2D.OverlapCircleAll( groundDetection.position, groundRadius, groundMask );
        foreach( var col in cols )
        {
            if( col.gameObject != gameObject )
            {
                ground = true;
                if( wasGrounded )
                {
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
        // in here we'll somehow simulate the dash animation... Should be interesting!
    }

    void Slide()
    {
        // in here we'll somehow simulate the sliding animation... Should be interesting!
    }

    void Interact() 
    {
        Debug.Log("The player has pressed interaction");
        onPlayerInteract?.Invoke( this );
    }

    void Melee( InputAction.CallbackContext context ) => onPlayerAttack?.Invoke( this, context.ReadValue<float>() > 0.5f );

    void Parry( InputAction.CallbackContext context ) 
    {
        onPlayerParry?.Invoke( this, context.ReadValue<float>() > 0.5f );
    }

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
            RaycastHit2D hit = Physics2D.CircleCast( groundDetection.position, groundRadius, -transform.up, Mathf.Infinity, groundMask ); 
            ground = false;
            Vector2 hitPointNormal = ( (Vector2)groundDetection.position - hit.point ).normalized;
            Vector2 jumpDir = ( Vector2.up + hitPointNormal ).normalized;
            _rb.AddForce( jumpDir * jumpForce, ForceMode2D.Impulse );
        }
    }

    public void OnHurt( int damages ) => _health.OnHurt( damages );

    public void OnHeal( int heal ) => _health.OnHeal( heal );

    #endregion
}
