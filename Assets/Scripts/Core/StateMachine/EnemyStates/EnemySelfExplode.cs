using System;
using Core.Pooling;
using UnityEngine;

namespace Core.StateMachine.EnemyStates
{
    public class EnemySelfExplode : MonoBehaviour, IState
    {
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private float timeAfterEffectDestroy = 3f;
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        public void Enter(object enterObject = null)
        {
            SelfExplodeWithEffect();
        }

        private void SelfExplodeWithEffect()
        {
            var particleEffect = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            particleEffect.Play();
            
            ParticleEffectManager.Instance.DestroyParticleEffectAfter(timeAfterEffectDestroy, particleEffect);
            
            ObjectPoolManager.ReturnObjectToPool(gameObject.transform.root.gameObject);
            // Destroy(gameObject.transform.root.gameObject);
        }

        public void Tick()
        {
        }

        public void FixedTick()
        {
            
        }

        public void Exit()
        {
            
        }
    }
}