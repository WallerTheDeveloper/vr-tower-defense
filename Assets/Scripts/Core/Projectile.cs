using Core.HealthSystem;
using UnityEngine;

namespace Core
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float lifetime = 3f;
        
        // [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private ParticleSystem collisionParticleEffect;
        [SerializeField] private float particleEffectLifetime = 1f;
        
        private Rigidbody rb;

        private TargetType targetType;
        
        private enum TargetType
        {
            Tower,
            Enemy,
            Unknown
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Initialize(GameObject target)
        {
            targetType = DetermineTargetType(target);
            Vector3 direction = (target.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * speed;
            Destroy(gameObject, lifetime);
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            // if (((1 << collision.gameObject.layer) & targetLayerMask) != 0)
            if(collision.gameObject.CompareTag("Enemy"))
            {
                var particleEffect = Instantiate(collisionParticleEffect, transform.position, Quaternion.identity);
                particleEffect.Play();
                HandleTargetHit(collision.gameObject, targetType);
                Destroy(particleEffect, particleEffectLifetime);
                Destroy(gameObject);
            }
        }

        private TargetType DetermineTargetType(GameObject target)
        {
            if (target.CompareTag("Tower"))
            {
                return TargetType.Tower;
            }

            if (target.CompareTag("Enemy"))
            {
                return TargetType.Enemy;
            }

            return TargetType.Unknown;
        }
        
        private void HandleTargetHit(GameObject target, TargetType targetType)
        {
            Health targetHealth = target.GetComponent<Health>();
            if (targetHealth != null)
            {
                // TODO: get damage from settings scriptable object (data provider?)
                targetHealth.TakeDamage(5f);
                
                switch (targetType)
                {
                    case TargetType.Tower:
                    {
                        Debug.Log("Hit a tower!");
                        break;
                    }
                    case TargetType.Enemy:
                    {
                        Debug.Log("Hit an enemy!");
                        break;
                    }
                }
            }
        }
    }
}