using System;
using Core.HealthSystem;
using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour.States
{
    public class TowerShooting : MonoBehaviour, IState
    {
        [SerializeField] private Transform firePoint;
        [SerializeField] private Projectile projectile;
        [SerializeField] private float fireRate = 5f; // Shots per second
        
        private Transform _currentTarget;
        
        private float _fireCooldown; // Time until the next shot is ready

        public bool IsStateActive { get; set; } = false;
        public event Action OnStateFinished;

        public void Enter()
        {
            IsStateActive = true;
        }
        public void Tick()
        {
            if (_currentTarget == null)
            {
                OnStateFinished?.Invoke();
                return;
            }
            if (_fireCooldown > 0)
            {
                _fireCooldown -= Time.deltaTime;
            }
            
            TryToShoot();
        }

        public void FixedTick() {}

        public void Exit()
        {
            IsStateActive = false;
        }

        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }
        private void TryToShoot()
        {
            if (_fireCooldown <= 0f)
            {
                if (projectile == null || firePoint == null || _currentTarget == null)
                {
                    return;
                }
            
                var projectileCopy = Instantiate(projectile, firePoint.position, firePoint.rotation);

                projectileCopy.Initialize(_currentTarget.gameObject);

                _fireCooldown = 1f / fireRate;
            }
        }
    }
}