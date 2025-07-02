using System;

namespace Core.StateMachine
{
    public interface IState
    {
        public bool IsStateFinished { get; set; }
        public event Action OnStateFinished;
        public void Enter();
        public void Tick();
        public void FixedTick();
        public void Exit();
    }
}