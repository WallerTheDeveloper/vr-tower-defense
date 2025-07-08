using System;
using Core.Data;
using UnityEngine;

namespace Core.HealthSystem
{
    public class Health : MonoBehaviour
    {
        private FlyingEnemySettings _flyingEnemySettings;
        private float currentHealth;
        public float HealthPercentage => currentHealth / _flyingEnemySettings.MaxHealth;
        public bool IsAlive => currentHealth > 0f;
        
        public event Action<float> OnHealthChanged;
        public event Action<float> OnDamageTaken;
        public event Action OnDeath;
        
        public void Initialize(FlyingEnemySettings flyingEnemySettings)
        {
            _flyingEnemySettings = flyingEnemySettings;
            currentHealth = flyingEnemySettings.MaxHealth;
        }
        
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
                
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, _flyingEnemySettings.MaxHealth);
            
            OnDamageTaken?.Invoke(damage);
            OnHealthChanged?.Invoke(currentHealth);
            
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        private void Die()
        {
            var deathParticleSystem = Instantiate(_flyingEnemySettings.DeathParticleSystem, transform.position, Quaternion.identity);
            deathParticleSystem.Play();
            Destroy(deathParticleSystem, _flyingEnemySettings.DeathParticleSystemLifetime);
            
            OnDeath?.Invoke();
            
            Destroy(gameObject);
        }
    }
}