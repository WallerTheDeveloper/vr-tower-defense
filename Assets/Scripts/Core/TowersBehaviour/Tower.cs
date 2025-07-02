using Core.StateMachine;
using UnityEngine;

namespace Core.TowersBehaviour
{
    public abstract class Tower : MonoBehaviour
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