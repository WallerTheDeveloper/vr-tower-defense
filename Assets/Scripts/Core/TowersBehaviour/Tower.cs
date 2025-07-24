using System;
using Core.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.TowersBehaviour
{
    public abstract class Tower : MonoBehaviour
    {
        [SerializeField] private GameObject towerHead;
        [SerializeField] private float radius = 20f;
        [SerializeField] private float anglePerSecond = 200f;
        [SerializeField] private LayerMask targetLayer;

        protected Action<Transform> OnTargetFound;
        private StateMachine.StateMachine _towerStateMachine = new();
        
        protected Transform currentTarget = null;
        protected abstract void Initialize();
        protected abstract void Tick();
        protected abstract void FixedTick();
        protected abstract void Deinitialize();
        
        protected void ChangeState(IState newState)
        {
            Debug.Log($"Current State: {newState}");
            _towerStateMachine.ChangeState(newState);
        }

        protected bool IsTargetWithinRange()
        {
            return currentTarget != null || currentTarget != FindNewTarget();
        }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Update()
        {
            Tick();
            RotateTowardsTarget();

            if (currentTarget != null)
            {
                if (IsLookingAtTarget(currentTarget.transform, Mathf.Infinity, 1f))
                {
                    OnTargetFound?.Invoke(currentTarget);
                }   
            }
        }

        private void FixedUpdate()
        {
            FixedTick();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }
        
        private void RotateTowardsTarget()
        {
            if (currentTarget == null)
            { 
                currentTarget = FindNewTarget();
                return;
            }
            
            var targetRotation = Quaternion.LookRotation(currentTarget.transform.position - towerHead.transform.position);
            towerHead.transform.rotation = Quaternion.RotateTowards(towerHead.transform.rotation, targetRotation, anglePerSecond * Time.deltaTime);
        }
        
        private Transform FindNewTarget()
        {
            Collider[] results = new Collider[5];
            var targets = Physics.OverlapSphereNonAlloc(transform.position, radius, results, targetLayer);
            // Debug.Log($"Found {targets} targets in radius {radius}");
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
            
            return closestEnemy;
        }

        private bool IsLookingAtTarget(Transform target, float maxDistance = 10f, float angleThreshold = 45f)
        {
            Vector3 directionToTarget = target.position - towerHead.transform.position;
            float distance = directionToTarget.magnitude;
        
            if (distance > maxDistance)
                return false;
        
            directionToTarget.Normalize();
            float dot = Vector3.Dot(towerHead.transform.forward, directionToTarget);
            float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
        
            return angle <= angleThreshold;
        }
    }
}