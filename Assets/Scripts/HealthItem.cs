using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D)), DisallowMultipleComponent]
public class HealthItem : MonoBehaviour
{
    public enum itemType { FullRestore, TwentyPercent_Heal};

    public itemType healthItemType;

#region Unity Events

    // Start is called before the first frame update
    void Start()
    {
        // force this object to be trigger
        if( TryGetComponent<Collider2D>(out Collider2D _col ))
            _col.isTrigger = true;
    }

    void OnDisable() => StopAllCoroutines();
    
    void OnTriggerEnter2D(Collider2D other )
    {
        if (other.TryGetComponent<IDamagable>(out IDamagable target) && target.health.health < target.health.maxHealth)
        {

            if (healthItemType == itemType.TwentyPercent_Heal)
                target.OnHeal((int)(target.health.maxHealth * 0.20f));

            if (healthItemType == itemType.FullRestore)
                target.OnHeal(target.health.maxHealth);

            gameObject.SetActive(false);
        }
    }

#endregion
}
