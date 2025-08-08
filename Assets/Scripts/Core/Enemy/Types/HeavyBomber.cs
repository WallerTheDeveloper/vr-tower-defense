using Core.Enemy.States;
using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Enemy.Types
{
    public class HeavyBomber: Unit
    {
        [SerializeField] private LayerMask finalEnemyLayer;
        [SerializeField] private GameObject statesLayer;    
        private EnemyFlyTowardsTarget _flyTowardsTargetState;
        private EnemyAttack _enemyAttackState;
        private SelfExplode _selfExplodeState;
        
        private bool _changedStateToFlyTowardsTarget = false;
        private bool _attackingFinalTarget = false;

        protected override void Initialize()
        {
            _flyTowardsTargetState = statesLayer.GetComponent<EnemyFlyTowardsTarget>();
            _enemyAttackState = statesLayer.GetComponent<EnemyAttack>();
            _selfExplodeState = statesLayer.GetComponent<SelfExplode>();
            
            _flyTowardsTargetState.OnStateFinished += ChangeToEnemyAttackState;
            _enemyAttackState.OnStateFinished += ChangeToFlyingTowardsTargetState;
        }

        protected override void Tick()
        {
            if (currentTarget != null && !_changedStateToFlyTowardsTarget)
            {
                base.ChangeState(_flyTowardsTargetState, base.currentTarget);
                _changedStateToFlyTowardsTarget = true;
            }

            // Meaning we check if we're attack target after death of which ship will self explode
            if (!_attackingFinalTarget && currentTarget != null && ((1 << currentTarget.gameObject.layer) & finalEnemyLayer) != 0)
            {
                _attackingFinalTarget = true;
            }
            
            // Final target has been destroyed
            if (_attackingFinalTarget && currentTarget == null)
            {
                base.ChangeState(_selfExplodeState);
            }
        }
        
        protected override void FixedTick()
        {
        }

        protected override void Deinitialize()
        {
            _flyTowardsTargetState.OnStateFinished -= ChangeToEnemyAttackState;
            _enemyAttackState.OnStateFinished -= ChangeToFlyingTowardsTargetState;
        }
        
        private void ChangeToEnemyAttackState()
        {
            base.ChangeState(_enemyAttackState, base.currentTarget);
        }
        private void ChangeToFlyingTowardsTargetState()
        {
            base.ChangeState(_flyTowardsTargetState, base.currentTarget);
        }
    }
}