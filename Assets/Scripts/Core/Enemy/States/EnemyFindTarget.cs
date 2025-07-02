using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.Enemy.States
{
    public class EnemyFindTarget : MonoBehaviour, IState
    {
        [SerializeField] private float radius = 20f;
        [SerializeField] private LayerMask targetLayer;
     
        private Transform _currentTarget;
        
        public bool IsStateFinished { get; set; }
        public event Action OnStateFinished;
        public event Action<Transform> OnTargetFound;
        public void Enter()
        {
            IsStateFinished = false;
        }

        public void Tick()
        {
            if (IsStateFinished)
            {
                return;
            }
            
            if (_currentTarget == null)
            {
                FindNewTarget();
            }
            else
            {
                OnStateFinished?.Invoke();
                OnTargetFound?.Invoke(_currentTarget);
                IsStateFinished = true;
            }
        }

        public void FixedTick() {}

        public void Exit()
        {
            IsStateFinished = true;
        }
        
        private Transform FindNewTarget()
        {
            Collider[] results = new Collider[5];
            int targetCount = Physics.OverlapSphereNonAlloc(transform.position, radius, results, targetLayer);
    
            Debug.Log($"Found {targetCount} targets in radius {radius}");
            Debug.Log($"Searching on layer mask: {targetLayer.value}");
    
            Transform closestTarget = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < targetCount; i++)
            {
                var targetCollider = results[i];
                Debug.Log($"Target {i}: {targetCollider.name}");
        
                float distance = Vector3.Distance(transform.position, targetCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = targetCollider.transform;
                }
            }

            _currentTarget = closestTarget;
            return closestTarget;
        }
    }
}