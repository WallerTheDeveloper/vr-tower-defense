using Core.TowersBehaviour.States;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowersBehaviour
{
    public class RayTower : Tower
    {
        [SerializeField] private GameObject statesLayer;
        
        // States
        private TowerAutoPlacement _autoPlacementState;
        private LaserAttack _attackState;
        private TowerIdle _idleState;
        private PrepareToAttack _prepareState;

        protected override void Initialize()
        {
            _prepareState = statesLayer.GetComponent<PrepareToAttack>();
            _autoPlacementState = statesLayer.GetComponent<TowerAutoPlacement>();
            _attackState = statesLayer.GetComponent<LaserAttack>();
            _idleState = statesLayer.GetComponent<TowerIdle>();

            base.ChangeState(_autoPlacementState);

            _autoPlacementState.OnStateFinished += OnAutoPlacementStateFinished;
            _prepareState.OnStateFinished += OnPrepareStateFinished;
        }

        protected override void Tick()
        {
            if (currentTarget == null && !_autoPlacementState.IsStateActive && !_idleState.IsStateActive)
            {
                base.ChangeState(_idleState);
            }
            if (currentTarget != null && !_prepareState.IsStateActive && !_attackState.IsStateActive && !_autoPlacementState.IsStateActive)
            {
                base.ChangeState(_prepareState, base.currentTarget);
            }
        }

        protected override void FixedTick()
        {
            if (_autoPlacementState.IsStateActive)
            { 
                _autoPlacementState.FixedTick();
            }
        }
        
        protected override void Deinitialize()
        {
            _autoPlacementState.OnStateFinished -= OnAutoPlacementStateFinished;
            _prepareState.OnStateFinished -= OnPrepareStateFinished;
        }
        
        private void OnAutoPlacementStateFinished()
        {
            var grabbableObject = GetComponent<XRGrabInteractable>();
            if (grabbableObject != null)
            {
                grabbableObject.enabled = false;
            }
            base.ChangeState(_idleState);
        }
        
        private void OnPrepareStateFinished()
        {
            base.ChangeState(_attackState, base.currentTarget);
        }
    }
}