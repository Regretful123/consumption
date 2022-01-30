using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraZoom : MonoBehaviour
{
    float m_orgValue = 0f;
    float m_targetValue = 0f;
    float m_timer = 0;
    bool m_isRunning = false;
    public float speed;
    [SerializeField] AnimationCurve tween = new AnimationCurve();

    Camera cam;

    void Start() => cam = Camera.main;

    public void NewOrthographicScale( float newOrthographicSize )
    {
        m_timer = 0;
        m_orgValue = cam.orthographicSize;
        m_targetValue = newOrthographicSize;
        if( !m_isRunning )
            StartCoroutine( PseudoUpdate() );
    }

    IEnumerator PseudoUpdate()
    {
        m_isRunning = true;
        while( m_timer < 1 )
        {
            m_timer += Time.deltaTime / speed;
            m_timer = Mathf.Min( m_timer, 1 );
            float percent = tween.Evaluate( m_timer );
            cam.orthographicSize = Mathf.LerpUnclamped( m_orgValue, m_targetValue, percent );
            yield return null;
        }
        m_isRunning = false;
    }
}
