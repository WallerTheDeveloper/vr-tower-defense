using System;

namespace Core.StateMachine
{
    public class StateMachine
    {
        private IState _currentState;

        public event Action<IState> OnStateChanged;
        
        public void ChangeState(IState newState)
        {
            _currentState?.Exit();

            _currentState = newState;
            OnStateChanged?.Invoke(_currentState);
            _currentState.Enter();
        }

        public void Tick()
        {
            _currentState?.Tick();
        }
    }
}