using System;
using Core.HealthSystem;
using Core.Pooling;
using Data;
using Data.Units;
using UnityEngine;

namespace Core
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 50f;
        [SerializeField] private float lifetime = 3f;
        
        [SerializeField] private LayerMask targetLayerMask;
        [SerializeField] private ParticleSystem collisionParticleEffect;
        [SerializeField] private float particleEffectLifetime = 2f;

        private BaseUnitSettings _shooterUnitSettings;
        private GameObject _target;
        private Rigidbody _rigidbody;
        private ParticleSystem _particleEffect;

        private bool _initialized = false;
        public void Initialize(BaseUnitSettings shooterUnitSettings)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _shooterUnitSettings = shooterUnitSettings;
        }
        public void ShootProjectile(GameObject target)
        {
            _target = target;
            
            var direction = (_target.transform.position - transform.position).normalized;
            
            transform.LookAt(target.transform);
            _rigidbody.linearVelocity = direction * speed;
            
            Destroy(gameObject, lifetime);

            _initialized = true;
        }

        private void Update()
        {
            if (_initialized && _target != null)
            {
                var direction = (_target.transform.position - transform.position).normalized;
                transform.rotation =  Quaternion.LookRotation(direction) * Quaternion.Euler(90, 0, 0);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            var targetObject = collision.collider.gameObject;
            bool targetCollider = ((1 << targetObject.layer) & targetLayerMask) != 0;

            if (targetCollider)
            {
                Health targetHealth = targetObject.GetComponent<Health>();
                targetHealth.TakeDamage(_shooterUnitSettings.Damage);
                
                _particleEffect = Instantiate(collisionParticleEffect, transform.position, Quaternion.identity);
                _particleEffect.Play();
                
                ParticleEffectManager.Instance.DestroyParticleEffectAfter(particleEffectLifetime, _particleEffect);
                
                Destroy(gameObject);
            }
        }
    }
}