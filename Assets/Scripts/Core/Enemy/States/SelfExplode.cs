using System;
using System.Collections;
using Core.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Enemy.States
{
    public class SelfExplode : MonoBehaviour, IState
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
            
            Destroy(gameObject.transform.root.gameObject);
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