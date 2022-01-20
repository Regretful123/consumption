using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(HealthComponent), typeof(Collider2D), typeof(Rigidbody2D)), DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    float move = 0f;
    bool ground = false;
    bool pauseControl = false;

    [Range(0, 100f)]
    public float speed = 5f;

    [Range(0, 100f)]
    public float jumpForce = 10f;

    public long experience = 0;
    
    [SerializeField] HealthComponent health;
    [SerializeField] LayerMask groundMask;

    public Action onPlayerDeath;
    public Action<PlayerController> onPlayerInteract;

    Mesh _mesh;
    Rigidbody2D _rb;

    GamePlayInputAction _gameplayInputAction;
    InputAction _movement;

#region Unity event

    void Start()
    {
        if( health == null )
            health = GetComponent<HealthComponent>() ?? gameObject.AddComponent<HealthComponent>();
        _mesh = GetComponent<Mesh>();
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
        CheckGround();
        move = _movement.ReadValue<Vector2>().x;
        Debug.Log(move);
        Vector2 _velocity = _rb.velocity;
        _velocity.x = move * speed;
        _rb.velocity = _velocity;
    }

#endregion

#region Input System Callback
    
    void OnMove(InputAction.CallbackContext context )
    {
        Vector2 val = context.ReadValue<Vector2>();
        Debug.Log( val );
        Move( val.x );
    }

    void OnJump( InputAction.CallbackContext context ) => Jump();

    void OnAttack( InputAction.CallbackContext context ) => Melee();

    void OnDash( InputAction.CallbackContext context ) => Dash();

    void OnExplosion( InputAction.CallbackContext context )
    {
        // throw new NotImplementedException();
    }

#endregion

    void HandlePlayerDeath() => onPlayerDeath?.Invoke();

    void CheckGround()
    { 
        Vector3 feet = transform.position;
        if( _mesh != null )
            feet += -transform.up * ( _mesh.bounds.size.y / 2 );
        Collider2D hit = Physics2D.OverlapCircle( feet, 1f, groundMask, 0, Mathf.Infinity );
        if( hit ) 
        {
            // check and see if we're technically facing down.
            // for now, we'll cheat a bit and just say that we're grounded.
            ground = true;
        }    
        else
        {
            ground = false;
        }
    }

    public void Move( float direction )
    {
        move = direction;
    }

    public void Dash()
    {

    }

    public void Slide()
    {

    }

    public void Interact() => onPlayerInteract?.Invoke( this );

    public void Melee()
    {
        // play melee animation
    }

    public void Jump()
    {
        if( ground )
        {
            _rb.AddForce( transform.up * jumpForce, ForceMode2D.Impulse );
            ground = false;
        }
    }
}
