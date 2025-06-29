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
        }
        
        protected override void Tick()
        {
            // TODO: auto placement must be blocking this
            if (_towerRotate.enabled && !_autoPlacement.enabled)
            {
                _towerRotate.Tick();
            }
        }

        protected override void FixedTick()
        {
            if (_autoPlacement.enabled)
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
        
        protected override void Deinitialize()
        {
        }
    }
}