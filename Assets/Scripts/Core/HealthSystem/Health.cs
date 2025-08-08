using Data;
using UnityEngine;

namespace Core.HealthSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private BaseUnitSettings baseUnitSettings;
        private float currentHealth;
        public float HealthPercentage => currentHealth / baseUnitSettings.MaxHealth;
        public bool IsAlive => currentHealth > 0f;

        private void Awake()
        {
            currentHealth = baseUnitSettings.MaxHealth;
        }
 
        public void TakeDamage(float damage)
        {
            if (!IsAlive)
            {
                return;
            }
                
            currentHealth -= damage;
            currentHealth = Mathf.Clamp(currentHealth, 0f, baseUnitSettings.MaxHealth);
            
            if (currentHealth <= 0f)
            {
                Die();
            }
        }
        
        private void Die()
        {
            var deathParticleSystem = Instantiate(baseUnitSettings.DeathParticleSystem, transform.position, Quaternion.identity);
            deathParticleSystem.Play();
            
            ParticleEffectManager.Instance.DestroyParticleEffectAfter(baseUnitSettings.DeathParticleSystemLifetime, deathParticleSystem);
            
            Destroy(gameObject);
        }
    }
}