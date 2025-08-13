using System;
using Core.HealthSystem.UI;
using Core.Pooling;
using Data.Units;
using UnityEngine;

namespace Core.HealthSystem
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private BaseUnitSettings baseUnitSettings;
        [SerializeField] private HealthBarView view;
        private HealthModel _model;

        private void OnEnable()
        {
            _model = new HealthModel(baseUnitSettings, baseUnitSettings.MaxHealth);
            _model.OnDeath += OnDeath;
            
            view.Initialize(baseUnitSettings.MaxHealth);
        }

        public void TakeDamage(float damage)
        {
            _model.TakeDamage(damage);
        }

        public void UpdateHealthView()
        {
            view.UpdateHealthBar(_model.HealthPercentage);
        }
        private void OnDeath()
        {
            ParticleSystem deathParticleSystem = Instantiate(baseUnitSettings.DeathParticleSystem, transform.position, Quaternion.identity);
            deathParticleSystem.Play();
            
            ParticleEffectManager.Instance.DestroyParticleEffectAfter(baseUnitSettings.DeathParticleSystemLifetime, deathParticleSystem);
            if (LayerMask.LayerToName(gameObject.layer) == "Headquarters")
            {
                Destroy(gameObject.transform.parent.gameObject);
                GameController.IsGameOver = true;
                return;
            }
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

        private void OnDisable()
        {
            _model.OnDeath -= OnDeath;
        }
    }
}