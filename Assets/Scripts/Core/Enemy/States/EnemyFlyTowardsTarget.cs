using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.Enemy.States
{
    public class EnemyFlyTowardsTarget : MonoBehaviour, IState
    {
        [SerializeField] private Transform parentTransform;
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float targetReachThreshold = 1f;
        
        private Transform _currentTarget;
        
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        
        public void Enter()
        {
            IsStateActive = false;
        }

        public void Tick()
        {
            float currentDistance = Vector3.Distance(parentTransform.position, _currentTarget.position);
            
            if (currentDistance > targetReachThreshold)
            {
                FlyTowards();
            }
            else
            {
                IsStateActive = true;
                OnStateFinished?.Invoke();
            }
            
            RotateTowardsTarget();
        }

        public void FixedTick()
        {
        }

        public void Exit()
        {
            IsStateActive = true;
        }

        public void SetTarget(Transform target)
        {
            _currentTarget = target;
        }
        
        private void FlyTowards()
        {
            Vector3 direction = (_currentTarget.position - parentTransform.position).normalized;
            float currentDistance = Vector3.Distance(parentTransform.position, _currentTarget.position);
            
            float moveDistance = moveSpeed * Time.deltaTime;
            
            if (currentDistance - moveDistance < targetReachThreshold)
            {
                Vector3 targetPosition = _currentTarget.position - direction * targetReachThreshold;
                parentTransform.position = targetPosition;
            }
            else
            {
                parentTransform.position += direction * moveDistance;
            }
        }
        
        private void RotateTowardsTarget()
        {
            Vector3 direction = (_currentTarget.position - parentTransform.position).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            parentTransform.rotation = Quaternion.Slerp(parentTransform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
    }
}