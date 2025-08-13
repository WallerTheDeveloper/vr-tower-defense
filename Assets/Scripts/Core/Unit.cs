using Core.StateMachine;
using UnityEngine;
using System.Collections.Generic;

namespace Core
{
    [System.Serializable]
    public class LayerGroup
    {
        public string name;
        public LayerMask layerMask;
        public int priority; // Lower number = higher priority
    }

    public abstract class Unit : MonoBehaviour
    {
        [SerializeField] private float radius = 20f;
        [SerializeField] private LayerGroup[] layerGroups = new LayerGroup[]
        {
            new LayerGroup { name = "High Priority", layerMask = 0, priority = 1 },
            new LayerGroup { name = "Medium Priority", layerMask = 0, priority = 2 },
            new LayerGroup { name = "Low Priority", layerMask = 0, priority = 3 }
        };

        private StateMachine.StateMachine _stateMachine = new();
        
        protected Transform currentTarget = null;
        protected int currentTargetPriority = int.MaxValue;
        
        protected abstract void Initialize();
        protected abstract void Tick();
        protected abstract void FixedTick();
        protected abstract void Deinitialize();
        
        protected void ChangeState(IState newState, object enterObject = null)
        {
            Debug.Log($"Current State: {newState}");
            if (enterObject != null)
            {
                _stateMachine.ChangeState(newState, enterObject);
            }
            else
            {
                _stateMachine.ChangeState(newState);
            }
        }
        
        private void OnEnable()
        {
            Initialize();
            
            System.Array.Sort(layerGroups, (a, b) => a.priority.CompareTo(b.priority));
        }
        
        private void Update()
        {
            Tick();
            
            _stateMachine.Tick();
            
            var targetInfo = FindNewTargetWithPriority();
            
            if (ShouldSwitchTarget(targetInfo))
            {
                currentTarget = targetInfo.target;
                currentTargetPriority = targetInfo.priority;
                
                OnTargetChanged(currentTarget);
            }
            
            if (currentTarget != null && !currentTarget.gameObject.activeInHierarchy)
            {
                currentTarget = null;
                currentTargetPriority = int.MaxValue;
                OnTargetLost();
            }
        }
        
        private void FixedUpdate()
        {
            FixedTick();
        }

        private void OnDisable()
        {
            currentTarget = null;
            currentTargetPriority = int.MaxValue;
            Deinitialize();
        }
        
        private struct TargetInfo
        {
            public Transform target;
            public int priority;
            
            public TargetInfo(Transform target, int priority)
            {
                this.target = target;
                this.priority = priority;
            }
        }
        
        private TargetInfo FindNewTargetWithPriority()
        {
            foreach (var layerGroup in layerGroups)
            {
                if (layerGroup.layerMask == 0) // Skip empty layer masks
                {
                    continue;
                }
                
                Transform target = FindClosestTarget(layerGroup.layerMask);
                if (target != null)
                {
                    return new TargetInfo(target, layerGroup.priority);
                }
            }
            
            return new TargetInfo(null, int.MaxValue);
        }
        
        private bool ShouldSwitchTarget(TargetInfo newTargetInfo)
        {
            if (newTargetInfo.target == null)
            {
                return false;
            }
            
            if (currentTarget == null)
            {
                return true;
            }
            
            if (currentTarget == newTargetInfo.target)
            {
                return false;
            }
            
            if (newTargetInfo.priority < currentTargetPriority)
            {
                Debug.Log($"Switching to higher priority target. Old priority: {currentTargetPriority}, New priority: {newTargetInfo.priority}");
                return true;
            }
            
            if (!IsTargetInRange(currentTarget))
            {
                return true;
            }
            
            return false;
        }
        
        private bool IsTargetInRange(Transform target)
        {
            if (target == null) return false;
            
            float distance = Vector3.Distance(transform.position, target.position);
            return distance <= radius;
        }
        
        private Transform FindClosestTarget(LayerMask layerMask)
        {
            Collider[] results = new Collider[20];
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, radius, results, layerMask);
            
            Transform closestTarget = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                if (results[i] == null)
                {
                    continue;
                }
                
                float distance = Vector3.Distance(transform.position, results[i].transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestTarget = results[i].transform;
                }
            }
            
            return closestTarget;
        }
        
        protected virtual void OnTargetChanged(Transform newTarget)
        {
            Debug.Log($"Target changed to: {newTarget?.name} (Priority: {currentTargetPriority})");
            currentTarget = newTarget;
        }
        
        protected virtual void OnTargetLost()
        {
            Debug.Log("Target lost");
        }
        
        public void ForceTargetRefresh()
        {
            var targetInfo = FindNewTargetWithPriority();
            if (targetInfo.target != null)
            {
                currentTarget = targetInfo.target;
                currentTargetPriority = targetInfo.priority;
                OnTargetChanged(currentTarget);
            }
        }
    }
}