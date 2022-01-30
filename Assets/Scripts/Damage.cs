using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    public Health health { get; }
    void OnHurt( int damages );
    void OnHeal( int heal );
}

public abstract class Damagable : MonoBehaviour, IDamagable
{
    protected Health m_health;
    public int initialHealth = 10;
    public Health health => m_health;

    virtual public void Start() => m_health = new Health( initialHealth );

    virtual public void OnHeal(int heal) => m_health?.OnHeal( heal );

    virtual public void OnHurt(int damages) => m_health?.OnHurt( damages );
}
