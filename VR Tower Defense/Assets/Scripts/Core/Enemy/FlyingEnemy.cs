using Core.StateMachine;
using UnityEngine;

namespace Core.Enemy
{
    // TODO: Duplicate implementation - separate into "Unit" general class
    public abstract class FlyingEnemy : MonoBehaviour
    {
        private StateMachine.StateMachine _towerStateMachine = new();
        protected abstract void Initialize();
        protected abstract void Tick();
        protected abstract void FixedTick();
        protected abstract void Deinitialize();
        
        protected void ChangeState(IState newState)
        {
            _towerStateMachine.ChangeState(newState);
        }

        private void Awake()
        {
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