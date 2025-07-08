using Core.Data;
using Core.HealthSystem;
using Core.StateMachine;
using UnityEngine;

namespace Core.Enemy
{
    // TODO: Duplicate implementation - separate into "Unit" general class
    public abstract class FlyingEnemy : MonoBehaviour
    {
        [SerializeField] private FlyingEnemySettings flyingEnemySettings;

        private StateMachine.StateMachine _towerStateMachine = new();
        protected abstract void Initialize();
        protected abstract void Tick();
        protected abstract void FixedTick();
        protected abstract void Deinitialize();
        protected Health health;
        
        protected void ChangeState(IState newState)
        {
            _towerStateMachine.ChangeState(newState);
        }
        

        private void Awake()
        {
            health = GetComponent<Health>();
            if (health == null)
            {
                health = gameObject.AddComponent<Health>();
            }

            health.Initialize(flyingEnemySettings);
            Initialize();
        }
        private void Update()
        {
            Tick();
        }

        private void FixedUpdate()
        {
            FixedTick();
        }

        private void OnDestroy()
        {
            Deinitialize();
        }
    }
}