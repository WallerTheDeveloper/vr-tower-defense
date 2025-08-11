using System;
using Core.Factories;
using Data;
using Data.Units;
using UnityEngine;

namespace Core.StateMachine.TowerStates
{
    public class TowerProjectileAttack : MonoBehaviour, IState
    {
        [SerializeField] private GameObject towerHead;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 5f;
        [SerializeField] private BaseUnitSettings unitSettings;
        
        [SerializeField] private ProjectileFactory projectileFactory;
        
        private Transform _currentTarget;
        
        private float _fireCooldown;

        public bool IsStateActive { get; set; } = false;
        public event Action OnStateFinished;

        public void Enter(object enterObject)
        {
            IsStateActive = true;
            _currentTarget = enterObject as Transform;
        }
        public void Tick()
        {
            if (_currentTarget == null)
            {
                Exit();
                return;
            }
            if (_fireCooldown > 0)
            {
                _fireCooldown -= Time.deltaTime;
            }
            
            towerHead.transform.LookAt(_currentTarget);
            TryToShoot();
        }

        public void FixedTick() {}
        
        public void Exit()
        {
            _currentTarget = null;
            IsStateActive = false;
            OnStateFinished?.Invoke();
        }

        private void TryToShoot()
        {
            if (_fireCooldown <= 0f)
            {
                if (firePoint == null || _currentTarget == null)
                {
                    return;
                }
            
                var projectile = projectileFactory.CreateProjectile(unitSettings, firePoint.position, firePoint.rotation);
                projectile.ShootProjectile(_currentTarget.gameObject);
                _fireCooldown = 1f / fireRate;
            }
        }
    }
}