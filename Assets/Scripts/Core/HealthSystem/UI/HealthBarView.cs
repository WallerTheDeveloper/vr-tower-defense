using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core.HealthSystem.UI
{
    public class HealthBarView : MonoBehaviour
    {
        [SerializeField] private Slider currentHealthSlider;
        [SerializeField] private Slider easeHealthSlider;
        [SerializeField] private float easeSliderChangeSpeed = 0.05f;
        private Camera _camera;

        public void Initialize(float initialHealth)
        {
            currentHealthSlider.value = initialHealth;
            easeHealthSlider.value = initialHealth;
            
            _camera = Camera.main;
        }
    
        public void UpdateHealthBar(float currentHealthPercentage)
        {
            currentHealthSlider.value = currentHealthPercentage;
        }
        
        private void Update()
        {
            transform.rotation = Quaternion.LookRotation(_camera.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
            if (!Mathf.Approximately(currentHealthSlider.value, easeHealthSlider.value))
            {
                easeHealthSlider.value = Mathf.Lerp(easeHealthSlider.value, currentHealthSlider.value, easeSliderChangeSpeed);
            }
        }
    }
}