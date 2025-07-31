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
        
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        public event Action<Transform> OnTargetFound;
        public void Enter(object enterObject)
        {
            IsStateActive = false;
        }

        public void Tick()
        {
            if (IsStateActive)
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
                IsStateActive = true;
            }
        }

        public void FixedTick() {}

        public void Exit()
        {
            IsStateActive = true;
        }
        
        private Transform FindNewTarget()
        {
            Collider[] results = new Collider[5];
            int targetCount = Physics.OverlapSphereNonAlloc(transform.position, radius, results, targetLayer);
    
            Transform closestTarget = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < targetCount; i++)
            {
                var targetCollider = results[i];
        
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