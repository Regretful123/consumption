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
    HashSet<PlayerController> _inTrigger = new HashSet<PlayerController>();

    void Start()
    {
        if( TryGetComponent<Collider2D>(out Collider2D _col ) )
            _col.isTrigger = true;
    }

    public virtual void OnOpen() { }
    public virtual void OnClose() { }

    virtual public void OnTriggerEnter2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            if( other.TryGetComponent<PlayerController>(out PlayerController _controller ))
            {
                _controller.onPlayerInteract += HandlePlayerInteract;
                _inTrigger.Add( _controller );
            }
        }
    }

    virtual public void OnTriggerExit2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            // for now handle when the player exit.
            if( other.TryGetComponent<PlayerController>(out PlayerController _controller ))
            {
                if( _inTrigger.Contains( _controller ))
                {
                    _controller.onPlayerInteract -= HandlePlayerInteract;
                    _inTrigger.Remove( _controller );
                }
            }

            if( !_stayOpen && _inTrigger.Count == 0 && _isOpen )
            {
                _isOpen = false;
                OnClose();
            }
        }
    }

    void HandlePlayerInteract(PlayerController _controller )
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
            target.onPlayerInteract -= HandlePlayerInteract;
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
