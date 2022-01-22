using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D)), DisallowMultipleComponent]
public class EnemyController : MonoBehaviour
{
    Transform _target;
    [SerializeField] Collider2D[] _colliders;
    Rigidbody2D _rb;
    [SerializeField, Range( 0, 100f )] float speed = 4f;
    [SerializeField] Transform groundCheck;
    [SerializeField] float groundRadius;
    [SerializeField] bool isGround;

    [SerializeField] Transform attackPoint;
    [SerializeField, Range( 0, 1 )] float attackDmgPercent = 1;

    public Action<EnemyController> onEnemyDeath;
    public Action<EnemyController> onEnemyArriveDestination;
    public Action<Vector3> onEnemyMovingTowardDestination;



    #region Unity Event

    void Awake()
    {
        _target = new GameObject(gameObject.name + "_target").transform;
    }

    void Start()
    {

    }

    #endregion 

}
