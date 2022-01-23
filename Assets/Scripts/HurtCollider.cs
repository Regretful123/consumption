using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public class HurtCollider : MonoBehaviour
{
    [SerializeField] float _damageOverTime = 0f;
    [SerializeField] int _damage = 10;
    [SerializeField] bool _isVisible = false;
    HashSet<IDamagable> _affected = new HashSet<IDamagable>();
    
    WaitForSeconds _dot;

#region Unity Events

    // Start is called before the first frame update
    void Start()
    {
        // force this object to be trigger
        if( TryGetComponent<Collider2D>(out Collider2D _col ))
            _col.isTrigger = true;
        
        // disable render if requested
        if( !_isVisible && TryGetComponent<Renderer>(out Renderer _render ))
            _render.enabled = false;
        _dot = new WaitForSeconds( _damageOverTime );
    }

    void OnEnable() => StartCoroutine(ApplyDamage());
    void OnDisable() => StopAllCoroutines();
    
    void OnTriggerEnter2D(Collider2D other )
    {
        if( other.TryGetComponent<IDamagable>(out IDamagable target ) && !_affected.Contains( target ))
            HandleAddTargetToDamagePool( target );
    }

    void OnTriggerExit2D(Collider2D other )
    {
        if( other.TryGetComponent<IDamagable>(out IDamagable target ) && _affected.Contains( target ))
            HandleRemovingTargetFromDamagePool( target );
    }

#endregion

#region Handler

    private void HandleHealthDepleted(Health _health)
    {
        IDamagable target = null;
        var query = _affected.Where( x => _health.Equals( x.health ));
        if( query.Any())
            target = query.FirstOrDefault();
        HandleRemovingTargetFromDamagePool( target );
    }

    void HandleAddTargetToDamagePool( IDamagable target )
    {
        if( target == null || _affected.Contains( target ))  
            return;
        
        target.health.onHealthDepleted += HandleHealthDepleted;
        _affected.Add( target );
    }

    void HandleRemovingTargetFromDamagePool( IDamagable target )
    {
        if( target == null || !_affected.Contains( target ))
            return;

        target.health.onHealthDepleted -= HandleHealthDepleted;
        _affected.Remove( target );
    }

#endregion

#region public interface

    public float GetDamageOverTime() => _damageOverTime;
    public void SetDamageOverTime( float seconds )
    {
        if( _damageOverTime == seconds )
            return;
        _damageOverTime = seconds;
        _dot = new WaitForSeconds( _damageOverTime );
    }

#endregion

#region private implementation

    IEnumerator ApplyDamage()
    {
        while( true )
        {
            while( _affected.Count == 0 )
                yield return null;
            yield return _dot;
            foreach( var target in _affected )
                target?.OnHurt(_damage);
        }
    }

#endregion

}
