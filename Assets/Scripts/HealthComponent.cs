using System;
using UnityEngine;
using UnityEngine.Events;

public interface IHealth
{
    void OnHurt( int impact );
    void OnHeal( int recover );
}

public class Health : IHealth
{
    int _maxHealth = 100;
    int _health = 100;  // could I ever get max health? what if I want to apply armor?
    public int health 
    { 
        get => _health;
        private set
        {
            int _val = value > 0 ? value : 0;   // clamp it to greater than or equal to 0
            _maxHealth = Mathf.Max( _maxHealth, _val );
            if( _health != _val )
            {
                _health = _val;
                if( _health == 0 )
                    onHealthDepleted?.Invoke();
                onHealthChanged?.Invoke( _health );
            }
        } 
    }

    private Health() { }
    public Health( int initialHealth )
    {
        this._health = initialHealth;
        this._maxHealth = initialHealth;
    }
    public event Action<int> onHealthChanged;
    public event Action onHealthDepleted;
    public int currentHealth => _health;
    public int maxHealth => _maxHealth;
    public void OnHurt( int impact ) => health -= impact;
    public void OnHeal( int recover ) => health += recover;
}

public class HealthComponent : MonoBehaviour
{

#region variables

    // private
    Health health;
    [SerializeField] int initialHealth = 100;
    
    // public
    public UnityEventInt onHealthChanged;
    public UnityEvent onHealthDepleted;

#endregion

#region unity events

    void Start()
    {
        health = new Health(initialHealth);
        health.onHealthChanged += HandleHealthChanged;
        health.onHealthDepleted += HandleHealthDepleted;
    }

    void OnDestroy()
    {
        health.onHealthChanged -= HandleHealthChanged;
        health.onHealthDepleted -= HandleHealthDepleted;
    }

#endregion

#region Handlers

    void HandleHealthDepleted() => onHealthDepleted?.Invoke();
    void HandleHealthChanged(int value) => onHealthChanged?.Invoke(value);

#endregion

#region public implementation
    
    public void OnHurt( int damages ) => health.OnHurt( damages );
    public void OnHeal( int recover ) => health.OnHeal( recover );

#endregion
}
