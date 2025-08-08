using System;
using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour.States
{
    public class PrepareToAttack : MonoBehaviour, IState
    {
        [SerializeField] private GameObject towerHead;
        [SerializeField] private float radius = 20f;
        [SerializeField] private float anglePerSecond = 200f;
        [SerializeField] private float aimTolerance = 5f;
        
        private Transform _currentTarget = null;
        private bool _isLookingAtTarget = false;
        public bool IsStateActive { get; set; }
        public event Action OnStateFinished;
        public void Enter(object enterObject = null)
        {
            IsStateActive = true;
            _currentTarget = enterObject as Transform;
        }

        public void Tick()
        {
            if (_currentTarget == null)
            {
                OnStateFinished?.Invoke();
                return;
            }
            bool lookingAtTarget = IsLookingAtTarget(_currentTarget.transform, aimTolerance);
            if (!lookingAtTarget)
            {
                RotateTowardsTarget(_currentTarget);
            }
            else
            {
                OnStateFinished?.Invoke();
            }
        }

        public void FixedTick() {}

        public void Exit()
        {
            IsStateActive = false;
        }
        
        private void RotateTowardsTarget(Transform target)
        {
            Vector3 directionToTarget = target.position - towerHead.transform.position;
            
            // Only rotate if there's a valid direction
            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                var targetRotation = Quaternion.LookRotation(directionToTarget);
                towerHead.transform.rotation = Quaternion.RotateTowards(towerHead.transform.rotation, targetRotation, anglePerSecond * Time.deltaTime);
            }
        }

        private bool IsLookingAtTarget(Transform targetTransform, float tolerance = 5f) // Changed default to 5 degrees
        {
            Vector3 directionToTarget = (targetTransform.position - towerHead.transform.position).normalized;
            Vector3 myForward = towerHead.transform.forward.normalized;
        
            float angle = Vector3.Angle(myForward, directionToTarget);
            return angle <= tolerance;
        }
    }
}