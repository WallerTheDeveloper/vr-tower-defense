using System;

namespace Core.StateMachine
{
    public interface IState
    {
        public void Enter();
        public void Tick();
        public void FixedTick();
        public void Exit();
        public event Action OnStateFinished;
    }
}