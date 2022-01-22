using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamagable
{
    Health health { get; }
    void OnHurt( int damages );
    void OnHeal( int heal );
}
