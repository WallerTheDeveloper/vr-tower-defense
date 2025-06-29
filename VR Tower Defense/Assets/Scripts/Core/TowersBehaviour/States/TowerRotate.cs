using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour.States
{
    public class TowerRotate : MonoBehaviour, IState
    {
        [SerializeField] private GameObject towerObject;
        [SerializeField] private float radius = 20f;
        [SerializeField] private float anglePerSecond = 200f;
        [SerializeField] private LayerMask targetLayer;

        public event Action OnStateFinished;
        
        private Transform _currentTarget;
        
        public void Enter() {}

        public void Tick()
        {
            RotateTowardsTarget();
        }

        public void FixedTick() {}

        public void Exit() {}
        
        private void RotateTowardsTarget()
        {
            if (_currentTarget != null)
            {
                if (IsLookingAtTarget(_currentTarget.transform, Mathf.Infinity, 10f))
                {
                    OnStateFinished?.Invoke();
                    this.enabled = false;
                    return;
                }
            }
            
            Transform closestEnemy = FindNewTarget();
            
            var targetRotation = Quaternion.LookRotation(closestEnemy.transform.position - towerObject.transform.position);
            towerObject.transform.rotation = Quaternion.RotateTowards(towerObject.transform.rotation, targetRotation, anglePerSecond * Time.deltaTime);
        }
        
        private Transform FindNewTarget()
        {
            Collider[] results = new Collider[5];
            var targets = Physics.OverlapSphereNonAlloc(transform.position, radius, results, targetLayer);
            Transform closestEnemy = null;
            float minDistance = float.MaxValue;

            foreach (var enemyCollider in results)
            {
                if (enemyCollider == null)
                {
                    continue;
                }
                
                float distance = Vector3.Distance(transform.position, enemyCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = enemyCollider.transform;
                }
            }

            _currentTarget = closestEnemy;
            
            return closestEnemy;
        }

        private bool IsLookingAtTarget(Transform target, float maxDistance = 10f, float angleThreshold = 45f)
        {
            Vector3 directionToTarget = target.position - towerObject.transform.position;
            float distance = directionToTarget.magnitude;
    
            if (distance > maxDistance)
                return false;
    
            directionToTarget.Normalize();
            float dot = Vector3.Dot(towerObject.transform.forward, directionToTarget);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
    
            return angle <= angleThreshold;
        }
    }
}