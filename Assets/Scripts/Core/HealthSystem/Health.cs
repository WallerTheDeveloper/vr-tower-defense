using Core.Pooling;
using Data;
using Data.Units;
using UnityEngine;

namespace Core.HealthSystem
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private BaseUnitSettings baseUnitSettings;
        private float currentHealth;
        public float HealthPercentage => currentHealth / baseUnitSettings.MaxHealth;
        public bool IsAlive => currentHealth > 0f;

        public void Awake()
        {
            currentHealth = baseUnitSettings.MaxHealth;
        }
 
        public void TakeDamage(float damage)
        {
            if (!IsAlive)
            {
                currentHealth = baseUnitSettings.MaxHealth;
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
            ParticleSystem deathParticleSystem = Instantiate(baseUnitSettings.DeathParticleSystem, transform.position, Quaternion.identity);
            deathParticleSystem.Play();
            
            ParticleEffectManager.Instance.DestroyParticleEffectAfter(baseUnitSettings.DeathParticleSystemLifetime, deathParticleSystem);
            
            // If game object has a parent it means that Health component is attached to child object 
            bool objectHasParent = gameObject.transform.parent != null;
            if (objectHasParent)
            {
                 Unit rootParent = gameObject.transform.GetComponentInParent<Unit>(); 
                 ObjectPoolManager.ReturnObjectToPool(rootParent.gameObject);
            }
            else
            {
                ObjectPoolManager.ReturnObjectToPool(gameObject);
            }
        }
    }
}