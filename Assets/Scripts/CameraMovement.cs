using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] float smooth = 0.5f;
    [SerializeField, Range(0,1)] float mouseInfluence = 0.2f; 
    Vector3 velocity = Vector3.zero;
    Camera cam;

    void Start() => cam = Camera.main;

    void LateUpdate()
    {   
        // get mouse position
        Vector3 target = Vector3.zero;

        // set z axis to camera's near clip plane
        target.z = cam.nearClipPlane;

        // convert mouse position into world point
        target = cam.ScreenToWorldPoint( Mouse.current.position.ReadValue() );

        target.z = transform.position.z;
        // interpolate between this target to mouse position by how the mouse influences over
        target = Vector3.Lerp( transform.position, target, mouseInfluence );

        // apply smooth damp motion
        cam.transform.position = Vector3.SmoothDamp( cam.transform.position, target, ref velocity, smooth );
    }
}
