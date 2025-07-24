using Core.TowersBehaviour.States;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowersBehaviour
{
    public class RocketLauncherTower : Tower
    {
        [SerializeField] private GameObject statesLayer;
        
        // States
        private TowerAutoPlacement _autoPlacementState;
        private TowerShooting _shootingState;
        private TowerIdle _idleState;
        protected override void Initialize()
        {
            _autoPlacementState = statesLayer.GetComponent<TowerAutoPlacement>();
            _shootingState = statesLayer.GetComponent<TowerShooting>();
            _idleState = statesLayer.GetComponent<TowerIdle>();
            
            base.ChangeState(_autoPlacementState);
         
            _autoPlacementState.OnStateFinished += OnAutoPlacementStateFinished;
        }

        protected override void Tick()
        {
            if (_shootingState.IsStateActive)
            {
                _shootingState.Tick();
            }

            if (_idleState.IsStateActive)
            {
                _idleState.Tick();
            }
            bool isTargetWithinRange = IsTargetWithinRange();
            
            if (!_autoPlacementState.IsStateActive && isTargetWithinRange)
            {
                _shootingState.SetTarget(base.currentTarget);
                if (!_shootingState.IsStateActive)
                {
                    base.ChangeState(_shootingState);
                }
            }
            else if(!_autoPlacementState.IsStateActive && !isTargetWithinRange && !_idleState.IsStateActive)
            {
                base.ChangeState(_idleState);
            }
        }

        protected override void FixedTick()
        {
            if (_autoPlacementState.IsStateActive)
            { 
                _autoPlacementState.FixedTick();
            }
        }
        
        private void OnAutoPlacementStateFinished()
        {
            var grabbableObject = GetComponent<XRGrabInteractable>();
            if (grabbableObject != null)
            {
                grabbableObject.enabled = false;
            }

            if (IsTargetWithinRange())
            {
                _shootingState.SetTarget(base.currentTarget);
                base.ChangeState(_shootingState);
            }
            else
            {
                base.ChangeState(_idleState);
            }
        }
        
        protected override void Deinitialize()
        {
            _autoPlacementState.OnStateFinished -= OnAutoPlacementStateFinished;
        }
    }
}