using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.Enemy.States
{
    public class EnemyAttack : MonoBehaviour, IState
    {
        [Header("Attack Settings")]
        [SerializeField] private Projectile projectilePrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 2f;
        [SerializeField] private float projectileSpeed = 10f;
        
        private Transform _currentTarget;
        private float _nextFireTime;
        
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        
        public void Enter()
        {
            IsStateActive = false;
            _nextFireTime = Time.time;
        }

        public void Tick()
        {
            if (_currentTarget == null)
            {
                IsStateActive = true;
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
        }
        
        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }
        
        private void Shoot()
        {
            if (projectilePrefab == null || firePoint == null || _currentTarget == null)
            {
                return;
            }
            
            Vector3 shootDirection = (_currentTarget.position - firePoint.position).normalized;
            
            var projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.LookRotation(shootDirection));
            
            projectile.Initialize(_currentTarget.gameObject);
            
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = shootDirection * projectileSpeed;
            }
        }
    }
}