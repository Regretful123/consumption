using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthIndicator : MonoBehaviour
{
    [SerializeField] Slider _slider;
    [SerializeField] PlayerController _playerController;    // not sure if this works? Let's find out!
    // Start is called before the first frame update
    void Start()
    {
        if( _slider == null )
        {
            if( !TryGetComponent<Slider>( out _slider ))
            {
                Debug.LogError("Unable to find Slider! Disabling!");
                this.enabled = false;
                return;
            }
        }

        if( _playerController == null )
        {
            Debug.LogWarning( "Health is not assigned! Will perform no action!");
            return;
        }

        HandleHealthChanged( _playerController.health.currentHealth );
        _playerController.health.onHealthChanged += HandleHealthChanged;
        _playerController.health.onHealthDepleted += HandleHealthDepleted;
    }

    void OnDestroy()
    {
        if( _playerController != null )
        {
            _playerController.health.onHealthChanged -= HandleHealthChanged;
            _playerController.health.onHealthDepleted -= HandleHealthDepleted;
        }
    }

    private void HandleHealthChanged(int value) => _slider.value = (float)value / _playerController.health.maxHealth;

    private void HandleHealthDepleted()
    {
        // do soemthing interesting? for now disable it...
        _slider.gameObject.SetActive(false);
    }
}
