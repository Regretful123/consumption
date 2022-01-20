using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    [SerializeField] long _health = 100;
    public long health
    {
        get => _health;
        set {
            _health = value > 0 ? value : 0;
            onHealthChanged?.Invoke( _health );
        }
    }

    public event Action onHealthDepleted;
    public event Action<long> onHealthChanged;

    public void Damage( long point )
    {
        health -= point;
        if( health == 0 )
            onHealthDepleted?.Invoke();
    }
}
