using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float speed = 0.5f;
    [SerializeField] float damp = 0.5f;
    Vector3 velocity = Vector3.zero;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void FixedUpdate()
    {
        cam.transform.position = Vector3.SmoothDamp( cam.transform.position, transform.position, ref velocity, damp, speed );
    }
}
