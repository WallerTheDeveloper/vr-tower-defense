using System;
using System.Collections;
using Core.HealthSystem;
using UnityEngine;

namespace Core.StateMachine.TowerStates
{
    [RequireComponent(typeof(AudioSource))]
    public class TowerLaserAttack : MonoBehaviour, IState
    {
        [Header("Laser Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private float damage = 25f;
        [SerializeField] private float damageInterval = 0.1f;
        
        [Header("Laser Visual")]
        [SerializeField] private LineRenderer laserLine;
        [SerializeField] private ParticleSystem laserStartEffect;
        [SerializeField] private ParticleSystem laserEndEffect;
        
        [Header("Audio")]
        [SerializeField] private AudioSource laserAudioSource;
        [SerializeField] private AudioClip laserSound;
        
        [SerializeField] private Transform towerHead;
        
        private Transform _currentTarget;
        private Coroutine _damageCoroutine;

        public bool IsStateActive { get; set; } = false;
        public event Action OnStateFinished;

        void Awake()
        {
            laserLine.enabled = false;
            laserLine.positionCount = 2;
        }

        public void Enter(object enterObject)
        {
            _currentTarget = enterObject as Transform;
            IsStateActive = true;
        }

        public void Tick()
        {
            if (_currentTarget == null)
            {
                OnStateFinished?.Invoke();
                return;
            }
            if (!laserLine.enabled)
            { 
                StartLaser();
            }
            
            towerHead.LookAt(_currentTarget);
            UpdateLaserVisuals();
        }

        public void FixedTick() {}

        public void Exit()
        {
            StopLaser();
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }
            _currentTarget = null;
            
            IsStateActive = false;
        }
        
        private void StartLaser()
        {
            laserLine.enabled = true;
                
            if (laserStartEffect != null)
            {
                laserStartEffect.Play();
            }
            
            laserAudioSource.clip = laserSound;
            laserAudioSource.loop = true;
            laserAudioSource.Play();

            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
            }
            _damageCoroutine = StartCoroutine(DealDamageOverTime());
        }
        
        private void StopLaser()
        {
            laserLine.enabled = false;

            if (laserStartEffect != null && laserEndEffect != null)
            {
                laserStartEffect.Stop();
                laserEndEffect.Stop();
            }
            
            laserAudioSource.Stop();
            
            if (_damageCoroutine != null)
            {
                StopCoroutine(_damageCoroutine);
                _damageCoroutine = null;
            }
        }
        
        private void UpdateLaserVisuals()
        {
            if (laserLine != null && firePoint != null && _currentTarget != null)
            {
                laserLine.SetPosition(0, firePoint.position);
                laserLine.SetPosition(1, _currentTarget.position);
                
                if (laserEndEffect != null)
                {
                    laserEndEffect.transform.position = _currentTarget.position;
                    laserEndEffect.Play();
                }
            }
        }
        
        private IEnumerator DealDamageOverTime()
        {
            while (_currentTarget != null)
            {
                HealthController targetHealthController = null;
                targetHealthController = _currentTarget.GetComponent<HealthController>();
                if (targetHealthController == null)
                {
                    targetHealthController = _currentTarget.GetComponentInParent<HealthController>();
                }
                targetHealthController.TakeDamage(damage);
                targetHealthController.UpdateHealthView();
                
                yield return new WaitForSeconds(damageInterval);
            }
        }
    }
}