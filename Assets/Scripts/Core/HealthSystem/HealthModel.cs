using System;
using Core.Pooling;
using Data.Units;
using UnityEngine;

namespace Core.HealthSystem
{
    public class HealthModel
    {
        private BaseUnitSettings _baseUnitSettings;
        private float _currentHealth;
        public float HealthPercentage => _currentHealth / _baseUnitSettings.MaxHealth;
        public bool IsAlive => _currentHealth > 0f;

        public Action OnDeath;
        
        public HealthModel(BaseUnitSettings baseUnitSettings, float currentHealth)
        {
            _baseUnitSettings = baseUnitSettings;
            _currentHealth = currentHealth;
        }
 
        public void TakeDamage(float damage)
        {
            if (!IsAlive)
            {
                _currentHealth = _baseUnitSettings.MaxHealth;
                return;
            }
                
            _currentHealth -= damage;
            _currentHealth = Mathf.Clamp(_currentHealth, 0f, _baseUnitSettings.MaxHealth);
            
            if (_currentHealth <= 0f)
            {
                OnDeath?.Invoke();
            }
        }
    }
}