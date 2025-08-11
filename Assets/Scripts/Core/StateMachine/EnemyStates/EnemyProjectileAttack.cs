using System;
using System.Collections.Generic;
using Core.Factories;
using Data;
using Data.Units;
using Unity.XR.CoreUtils.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.StateMachine.EnemyStates
{
    public class EnemyProjectileAttack : MonoBehaviour, IState
    {
        [Header("Attack Settings")]
        [SerializeField] private BaseUnitSettings unitSettings;
        [SerializeField] private float fireRate = 2f;

        [System.Serializable]
        public class FactorySpawnPoint
        {
            public ProjectileFactory factory;
            public Transform spawnPoint;
        }

        [SerializeField] private FactorySpawnPoint[] factorySpawnPoints;

        private Dictionary<ProjectileFactory, Transform> _factoryPoints;
        private Transform _currentTarget;
        private float _nextFireTime;
        
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        
        public void Enter(object enterObject)
        {
            _factoryPoints = GetFactoryPoints();
            _nextFireTime = Time.time;
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
            foreach (var (factory, firePoint) in _factoryPoints)
            {
                var projectile = factory.CreateProjectile(unitSettings, firePoint.position, firePoint.rotation);
                projectile.ShootProjectile(_currentTarget.gameObject);
            }
        }
        
        private Dictionary<ProjectileFactory, Transform> GetFactoryPoints()
        {
            var dict = new Dictionary<ProjectileFactory, Transform>();
    
            foreach (var entry in factorySpawnPoints)
            {
                if (entry.factory != null && entry.spawnPoint != null)
                {
                    dict[entry.factory] = entry.spawnPoint;
                }
            }
    
            return dict;
        }
    }
}