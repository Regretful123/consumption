using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public abstract class Door : MonoBehaviour
{
    bool _isOpen = false;
    bool _fireOnce = false;
    bool _hasFired = false;
    bool _stayOpen = false;
    public bool IsOpen() => _isOpen;
    HashSet<InputAction> _inTrigger = new HashSet<InputAction>();

    void Start()
    {
        if( TryGetComponent<Collider2D>(out Collider2D _col ) )
            _col.isTrigger = true;
    }

    public virtual void OnOpen() { }
    public virtual void OnClose() { }

    public void OnTriggerEnter2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            if( TryGetComponent<PlayerController>(out PlayerController _controller ))
            {
                InputAction _interact = _controller.GetGamePlayInputAction().Gameplay.Interact;
                _interact.performed += HandlePlayerInteract;
                _inTrigger.Add( _interact );
            }
        }
    }

    public void OnTriggerExit2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            // for now handle when the player exit.
            if( TryGetComponent<PlayerController>(out PlayerController _controller ))
            {
                InputAction _interact = _controller.GetGamePlayInputAction().Gameplay.Interact;
                if( _inTrigger.Contains( _interact ))
                {
                    _interact.performed -= HandlePlayerInteract;
                    _inTrigger.Remove( _interact );
                }
            }

            if( !_stayOpen && _inTrigger.Count == 0 && _isOpen )
            {
                _isOpen = false;
                OnClose();
            }
        }
    }

    void HandlePlayerInteract(InputAction.CallbackContext context )
    {
        // for now handle when the player enter
        if( !_isOpen && ( ( _fireOnce && !_hasFired ) || ( !_fireOnce ) )  )
        {
            _hasFired = true;
            _isOpen = true;
            OnOpen();
        }
    }

    // if the gameobject becomes disable, we need to make sure we free all of the handler reference.
    void OnDisable()
    {
        foreach( var target in _inTrigger )
            target.performed -= HandlePlayerInteract;
        _inTrigger.Clear();
    }
}

public class DoorController : Door
{
    public UnityEvent onDoorOpen;
    public UnityEvent onDoorClosed;

    public override void OnOpen() => onDoorOpen?.Invoke();
    public override void OnClose() => onDoorClosed?.Invoke();
}
