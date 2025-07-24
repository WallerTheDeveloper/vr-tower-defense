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
            _shootingState.OnStateFinished += ChangeToIdleState;
            base.OnTargetFound += SetTargetForShooting;
        }

        protected override void Tick()
        {
            if (_shootingState.IsStateActive)
            {
                _shootingState.Tick();
            }
            if (!_autoPlacementState.IsStateActive && IsTargetWithinRange() && !_shootingState.IsStateActive)
            {
                _shootingState.SetTarget(base.currentTarget);
                base.ChangeState(_shootingState);
            }
            else if(!_autoPlacementState.IsStateActive && !IsTargetWithinRange() && !_idleState.IsStateActive)
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

        private void ChangeToIdleState()
        {
            base.ChangeState(_idleState);
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
        
        private void SetTargetForShooting(Transform currentTarget)
        {
            _shootingState.SetTarget(currentTarget);
        }

        protected override void Deinitialize()
        {
            _autoPlacementState.OnStateFinished -= OnAutoPlacementStateFinished;
            _shootingState.OnStateFinished -= ChangeToIdleState;
            base.OnTargetFound -= SetTargetForShooting;
        }
    }
}