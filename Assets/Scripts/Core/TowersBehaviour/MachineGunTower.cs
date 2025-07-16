using Core.Factories;
using Core.TowersBehaviour.States;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowersBehaviour
{
    public class MachineGunTower : Tower
    {
        [SerializeField] private GameObject statesLayer;
        
        // Machine Gun States
        private TowerAutoPlacement _autoPlacement;
        private TowerRotate _towerRotate;
        private TowerShooting _towerShooting;
        
        protected override void Initialize()
        {
            _autoPlacement = statesLayer.GetComponent<TowerAutoPlacement>();
            _towerRotate = statesLayer.GetComponent<TowerRotate>();
            _towerShooting = statesLayer.GetComponent<TowerShooting>();
            
            base.ChangeState(_autoPlacement);
         
            _autoPlacement.OnStateFinished += ChangeToRotationState;
            _towerRotate.OnStateFinished += ChangeToShootingState;
            _towerRotate.OnTargetFound += currentTarget =>
            {
                _towerShooting.SetTarget(currentTarget);
            };
        }

        protected override void Tick()
        {
            if (_autoPlacement.IsStateFinished && !_towerRotate.IsStateFinished)
            {
                _towerRotate.Tick();
            }
            if (_towerRotate.IsStateFinished)
            {
                _towerShooting.Tick();
            }
        }

        protected override void FixedTick()
        {
            if (!_autoPlacement.IsStateFinished)
            { 
                _autoPlacement.FixedTick();
            }
        }

        private void ChangeToRotationState()
        {
            _autoPlacement.OnStateFinished -= ChangeToRotationState;
            
            var grabbableObject = GetComponent<XRGrabInteractable>();
            if (grabbableObject != null)
            {
                grabbableObject.enabled = false;
            }
            
            base.ChangeState(_towerRotate);
        }
        
        private void ChangeToShootingState()
        {
            _towerRotate.OnStateFinished -= ChangeToShootingState;
            base.ChangeState(_towerShooting);
        }
        
        protected override void Deinitialize()
        {
        }
    }
}