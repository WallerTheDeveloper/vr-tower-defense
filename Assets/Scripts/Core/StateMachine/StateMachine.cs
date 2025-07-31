using System;

namespace Core.StateMachine
{
    public class StateMachine
    {
        private IState _currentState;

        public event Action<IState> OnStateChanged;

        private bool _changingState = false;
        public void ChangeState(IState newState, object enterObject = null)
        {
            _changingState = true;
            
            _currentState?.Exit();

            _currentState = newState;
            
            if (enterObject != null)
            {
                _currentState.Enter(enterObject);
            }
            else
            {
                _currentState.Enter();
            }
            
            OnStateChanged?.Invoke(_currentState);
            
            _changingState = false;
        }

        public void Tick()
        {
            if (!_changingState)
            { 
                _currentState?.Tick();
            }
        }
    }
}