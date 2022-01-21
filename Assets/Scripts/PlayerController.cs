using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HealthComponent), typeof(Rigidbody2D)), DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    const float airStrafe = 0.35f;
    const float landStrafe = 0.1f;
    [SerializeField] float height = 1f;
    [SerializeField] float crouchHeight = 0.4f;
    [SerializeField, Range(0,1)] float crouchSpeed = 0.4f;

    bool ground = false;
    bool pauseControl = false;
    bool crouch = false;
    bool wasCrouching = false;
    bool freezeInput = false;

    [Range(0, 100f)] public float speed = 5f;

    float _moveSmooth = 0.05f;

    [SerializeField, Range(0, 100f)] float jumpForce = 10f;

    public long experience { get; private set; } = 0;
    
    [SerializeField] HealthComponent health;
    [SerializeField, Range(0.01f, 10f)] float ceilingRadius;
    [SerializeField, Range(0.01f, 5f)] float _groundRadius;
    [SerializeField] Transform ceilingDetection;
    [SerializeField] Transform _groundDetection;
    [SerializeField] LayerMask groundMask;

    public Action onPlayerDeath;
    public Action<PlayerController> onPlayerInteract;
    public Action<PlayerController> onPlayerLand;
    public Action<PlayerController> onPlayerCrouch;
    public Action<PlayerController> onPlayerStand;
    public Action<PlayerController> onPlayerAttack;

    Rigidbody2D _rb;
    Vector3 _velocity = Vector3.zero;
    Vector3 _targetVelocity = Vector3.zero;

    GamePlayInputAction _gameplayInputAction;
    InputAction _movement;

#region Unity event

    void Start()
    {
        if( health == null )
            health = GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
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
        _gameplayInputAction.Gameplay.Explosion.performed += OnExplosion;
        _gameplayInputAction.Gameplay.Explosion.Enable();

        health.onHealthDepleted += HandlePlayerDeath;
    }

    void OnDestroy()
    {
        if( health != null )
            health.onHealthDepleted -= HandlePlayerDeath;
        _gameplayInputAction.Gameplay.Jump.performed -= OnJump;
        _gameplayInputAction.Gameplay.Jump.Enable();
        _gameplayInputAction.Gameplay.Attack.performed -= OnAttack;
        _gameplayInputAction.Gameplay.Attack.Enable();
        _gameplayInputAction.Gameplay.Dash.performed -= OnDash;
        _gameplayInputAction.Gameplay.Dash.Enable();
        _gameplayInputAction.Gameplay.Explosion.performed -= OnExplosion;
        _gameplayInputAction.Gameplay.Explosion.Enable();
    }

    void FixedUpdate()
    {
        CheckCeiling();
        CheckGround();
        
        // if for whatever reason (Stun/bonk/unconscious) we need to be able to control this.
        if( ground || !pauseControl )
            Move();
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

#endregion

#region Player Implementation

    void HandlePlayerDeath() => onPlayerDeath?.Invoke();

    
    void CheckCeiling()
    {
        if( crouch )
            return;
        
        if( Physics2D.OverlapCircle( ceilingDetection.position, ceilingRadius, groundMask))
            crouch = true;
    }


    void CheckGround()
    { 
        bool wasGrounded = ground;
        ground = false;

        Collider2D[] cols = Physics2D.OverlapCircleAll( _groundDetection.position, _groundRadius, groundMask );
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
                onPlayerCrouch?.Invoke(this);
            }
            _rb.AddForce( Physics2D.gravity * 0.5f, ForceMode2D.Impulse );
        }
        else if( input.y >= 0f && crouch )
        {
            crouch = false;
            if( wasCrouching )
            {
                wasCrouching = false;
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
    }

    void Jump()
    {
        if( ground )
        {
            ground = false;
            _rb.AddForce( transform.up * jumpForce, ForceMode2D.Impulse );
        }
    }

    #endregion
}
