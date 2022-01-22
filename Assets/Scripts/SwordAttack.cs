using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public class SwordAttack : MonoBehaviour
{
    [SerializeField] int _damage = 5;
    [SerializeField] LayerMask damageMask;
    
    // Start is called before the first frame update
    void Start()
    {
        // ensure that the collision is a trigger field.
        if( TryGetComponent<Collider2D>(out Collider2D _col ))
            _col.isTrigger = true;       
    }

    void OnTriggerEnter2D( Collider2D other )
    {
        // for now let's skip the damage collision and focus on getting this to work.
        if( other.gameObject.Equals( gameObject ))
            return; // don't hurt yourself stupid...

        if( other.TryGetComponent<IDamagable>(out IDamagable _affected))
            _affected.OnHurt( _damage );
    }
}
