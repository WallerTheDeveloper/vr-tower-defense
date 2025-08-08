using System;
using Core.Factories;
using Core.StateMachine;
using Data;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Enemy.States
{
    public class EnemyAttack : MonoBehaviour, IState
    {
        [Header("Attack Settings")]
        [SerializeField] private Transform firePoint;
        [SerializeField] private BaseUnitSettings unitSettings;
        [SerializeField] private float fireRate = 2f;

        [SerializeField] private ProjectileFactory projectileFactory;
        
        private Transform _currentTarget;
        private float _nextFireTime;
        
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        
        public void Enter(object enterObject)
        {
            IsStateActive = true;
            _nextFireTime = Time.time;
            _currentTarget = enterObject as Transform;
        }

        public void Tick()
        {
            if (_currentTarget == null)
            { 
                OnStateFinished?.Invoke();
                return;
            }
            
            if (Time.time >= _nextFireTime)
            {
                Shoot();
                _nextFireTime = Time.time + (1f / fireRate);
            }
        }

        public void FixedTick()
        {
        }

        public void Exit()
        {
            _nextFireTime = 0f;
            IsStateActive = false;
        }
        
        private void Shoot()
        {
            if (firePoint == null || _currentTarget == null)
            {
                return;
            }

            var projectile = projectileFactory.CreateProjectile(unitSettings, firePoint.position, firePoint.rotation);
            projectile.ShootProjectile(_currentTarget.gameObject);
        }
    }
}