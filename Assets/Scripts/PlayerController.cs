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
    public Action<PlayerController> onPlayerAttack;
    public Action<PlayerController> onPlayerParry;

    public UnityEventInt onPlayerHealthChanged;
    public UnityEvent onPlayerDeath;

    Rigidbody2D _rb;
    Vector3 _velocity = Vector3.zero;
    Vector3 _targetVelocity = Vector3.zero;

    GamePlayInputAction _gameplayInputAction;
    InputAction _movement;
    public GamePlayInputAction GetGamePlayInputAction() => _gameplayInputAction;

#region Unity event

    void Awake()
    {
        _health = new Health( startingHealth );
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _gameplayInputAction = new GamePlayInputAction();

        _movement = _gameplayInputAction.Gameplay.Move;
        _movement.Enable();
        _gameplayInputAction.Gameplay.Jump.performed += OnJump;
        _gameplayInputAction.Gameplay.Jump.Enable();
        _gameplayInputAction.Gameplay.Attack.performed += OnAttack;
        _gameplayInputAction.Gameplay.Attack.Enable();
        _gameplayInputAction.Gameplay.Dash.performed += OnDash;
        _gameplayInputAction.Gameplay.Dash.Enable();
        _gameplayInputAction.Gameplay.Interact.performed += OnInteract;
        _gameplayInputAction.Gameplay.Interact.Enable();
        _gameplayInputAction.Gameplay.Explosion.performed += OnExplosion;
        _gameplayInputAction.Gameplay.Explosion.Enable();
        _health.onHealthDepleted += HandlePlayerDeath;
    }

    void OnDestroy()
    {
        _gameplayInputAction.Gameplay.Jump.performed -= OnJump;
        _gameplayInputAction.Gameplay.Jump.Disable();
        _gameplayInputAction.Gameplay.Attack.performed -= OnAttack;
        _gameplayInputAction.Gameplay.Attack.Disable();
        _gameplayInputAction.Gameplay.Dash.performed -= OnDash;
        _gameplayInputAction.Gameplay.Dash.Disable();
        _gameplayInputAction.Gameplay.Interact.performed -= OnInteract;
        _gameplayInputAction.Gameplay.Interact.Disable();
        _gameplayInputAction.Gameplay.Explosion.performed -= OnExplosion;
        _gameplayInputAction.Gameplay.Explosion.Disable();
        _health.onHealthDepleted -= HandlePlayerDeath;
    }

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

    #region Input System Callback

    void OnJump( InputAction.CallbackContext context ) => Jump();

    void OnAttack( InputAction.CallbackContext context ) => Melee();

    void OnDash( InputAction.CallbackContext context ) => Dash();

    void OnExplosion( InputAction.CallbackContext context )
    {
        // throw new NotImplementedException();
    }

    void OnInteract( InputAction.CallbackContext context ) => Interact();

    void OnParry( InputAction.CallbackContext context ) => Parry();

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

    void Interact() => onPlayerInteract?.Invoke( this );

    void Melee()
    {
        // play melee animation!
        onPlayerAttack?.Invoke( this );
    }

    void Parry() => onPlayerParry?.Invoke( this );

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

        // if( _displayMesh != null )
        // {
        //     Vector3 _scale = _displayMesh.localScale;
        //     if( ( _scale.x > 0 && move < 0 ) || ( _scale.x < 0 && move > 0 ) )
        //     {
        //         _scale.x *= -1;
        //         _displayMesh.localScale = _scale;
        //     }
        // }
        
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
