using System;
using Core.StateMachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.TowersBehaviour
{
    public abstract class Tower : MonoBehaviour
    {
        [SerializeField] private float radius = 20f;
        [SerializeField] private LayerMask targetLayer;

        private StateMachine.StateMachine _towerStateMachine = new();
        
        protected Transform currentTarget = null;
        protected abstract void Initialize();
        protected abstract void Tick();
        protected abstract void FixedTick();
        protected abstract void Deinitialize();
        
        protected void ChangeState(IState newState, object enterObject = null)
        {
            Debug.Log($"Current State: {newState}");
            if (enterObject != null)
            {
                _towerStateMachine.ChangeState(newState, enterObject);
            }
            else
            {
                _towerStateMachine.ChangeState(newState);
            }
        }
        
        private void Awake()
        {
            Initialize();
        }
        
        private void Update()
        {
            Tick();
            
            _towerStateMachine.Tick();

            Transform newTarget = FindNewTarget();
            if ((newTarget != null && newTarget != currentTarget) || currentTarget == null)
            {
                currentTarget = newTarget;
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
        
        private Transform FindNewTarget()
        {
            Collider[] results = new Collider[20];
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
    }
}