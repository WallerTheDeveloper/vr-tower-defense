using Core.StateMachine.TowerStates;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowerUnits
{
    public class RayTowerUnit : Unit
    {
        [SerializeField] private GameObject statesLayer;
        
        // States
        private TowerAutoPlacement _autoPlacementState;
        private TowerLaserAttack _attackState;
        private TowerIdle _idleState;
        private TowerPrepareToAttack _towerPrepareState;

        protected override void Initialize()
        {
            _towerPrepareState = statesLayer.GetComponent<TowerPrepareToAttack>();
            _autoPlacementState = statesLayer.GetComponent<TowerAutoPlacement>();
            _attackState = statesLayer.GetComponent<TowerLaserAttack>();
            _idleState = statesLayer.GetComponent<TowerIdle>();

            base.ChangeState(_autoPlacementState);

            _autoPlacementState.OnStateFinished += OnAutoPlacementStateFinished;
            _towerPrepareState.OnStateFinished += OnTowerPrepareStateFinished;
        }

        protected override void Tick()
        {
            if (currentTarget == null && !_autoPlacementState.IsStateActive && !_idleState.IsStateActive)
            {
                base.ChangeState(_idleState);
            }
            if (currentTarget != null && !_towerPrepareState.IsStateActive && !_attackState.IsStateActive && !_autoPlacementState.IsStateActive)
            {
                base.ChangeState(_towerPrepareState, base.currentTarget);
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
            _towerPrepareState.OnStateFinished -= OnTowerPrepareStateFinished;
        }
        
        private void OnAutoPlacementStateFinished()
        {
            base.ChangeState(_idleState);
        }
        
        private void OnTowerPrepareStateFinished()
        {
            base.ChangeState(_attackState, base.currentTarget);
        }
    }
}