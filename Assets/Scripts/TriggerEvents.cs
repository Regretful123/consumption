using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class UnityEventGameObject : UnityEvent<GameObject> { }

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public class TriggerEvents : MonoBehaviour
{
    public string filter = "Player";
    public UnityEventGameObject onTriggerEnter2D;
    public UnityEvent onTriggerExit2D;
    public UnityEventGameObject onCollisionEnter2D;
    public UnityEvent onCollisionExit2D;
   
    public void OnTriggerEnter2D( Collider2D other )
    {
        if( string.IsNullOrEmpty( filter ) || other.CompareTag( filter ) )
            if( other.gameObject != gameObject )
                onTriggerEnter2D?.Invoke( other.gameObject );
    }

    public void OnTriggerExit2D( Collider2D other ) 
    {
        if( string.IsNullOrEmpty( filter ) || other.CompareTag( filter ) )
            if( other.gameObject != gameObject )
                onTriggerExit2D?.Invoke();
    }

    public void OnCollisionEnter2D( Collision2D other ) 
    {
        if( string.IsNullOrEmpty( filter ) || other.gameObject.CompareTag( filter ) )
            if( other.gameObject != gameObject )
                onCollisionEnter2D?.Invoke( other.gameObject );
    }

    public void OnCollisionExit2D( Collision2D other ) 
    {
        if( string.IsNullOrEmpty( filter ) || other.gameObject.CompareTag( filter ) )
            if( other.gameObject != gameObject )
                onCollisionExit2D?.Invoke();
    }
}
