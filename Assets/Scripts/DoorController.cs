using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public abstract class Door : MonoBehaviour
{
    bool _isOpen = false;
    public bool IsOpen() => _isOpen;
    HashSet<GameObject> _inTrigger = new HashSet<GameObject>();

    void Start()
    {
        if( TryGetComponent<Collider2D>(out Collider2D _col ) )
            _col.isTrigger = true;
    }

    public virtual void OnOpen() { Debug.Log("Open"); }
    public virtual void OnClose() { Debug.Log("Close"); }

    public void OnTriggerEnter2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            _inTrigger.Add( other.gameObject );

            // for now handle when the player enter
            if( !_isOpen )
            {
                _isOpen = true;
                OnOpen();
            }
        }
    }

    public void OnTriggerExit2D( Collider2D other )
    {
        if( other.CompareTag("Player"))
        {
            // for now handle when the player exit.
            _inTrigger.Remove( other.gameObject );

            if( _inTrigger.Count == 0 && _isOpen )
            {
                _isOpen = false;
                OnClose();
            }
        }
    }
}

public class DoorController : Door
{
    public UnityEvent onDoorOpen;
    public UnityEvent onDoorClosed;

    public override void OnOpen() => onDoorOpen?.Invoke();
    public override void OnClose() => onDoorClosed?.Invoke();
}
