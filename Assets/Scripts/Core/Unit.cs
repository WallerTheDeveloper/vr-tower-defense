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
        
        private void Awake()
        {
            Initialize();
            
            // Sort layer groups by priority at start
            System.Array.Sort(layerGroups, (a, b) => a.priority.CompareTo(b.priority));
        }
        
        private void Update()
        {
            Tick();
            
            _stateMachine.Tick();

            Transform newTarget = FindNewTarget();
            if (newTarget != null && newTarget != currentTarget)
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
            // Search through layer groups in priority order
            foreach (var layerGroup in layerGroups)
            {
                if (layerGroup.layerMask == 0) continue; // Skip empty layer masks
                
                Transform target = FindClosestTarget(layerGroup.layerMask);
                if (target != null)
                {
                    return target;
                }
            }
            
            return null;
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
        
        // Helper method to add/modify layer groups at runtime
        public void SetLayerGroup(int index, LayerMask layerMask, int priority)
        {
            if (index >= 0 && index < layerGroups.Length)
            {
                layerGroups[index].layerMask = layerMask;
                layerGroups[index].priority = priority;
                
                // Re-sort after modification
                System.Array.Sort(layerGroups, (a, b) => a.priority.CompareTo(b.priority));
            }
        }
    }
}