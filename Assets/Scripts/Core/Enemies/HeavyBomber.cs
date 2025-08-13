using Core.StateMachine.EnemyStates;
using UnityEngine;

namespace Core.Enemies
{
    public class HeavyBomber: Unit
    {
        [SerializeField] private LayerMask finalEnemyLayer;
        [SerializeField] private GameObject statesLayer;    
        private EnemyFlyTowardsTarget _enemyFlyTowardsTargetState;
        private EnemyProjectileAttack _enemyProjectileAttackState;
        private EnemySelfExplode _enemySelfExplodeState;
        
        private bool _changedStateToFlyTowardsTarget = false;
        private bool _attackingFinalTarget = false;

        protected override void Initialize()
        {
            _enemyFlyTowardsTargetState = statesLayer.GetComponent<EnemyFlyTowardsTarget>();
            _enemyProjectileAttackState = statesLayer.GetComponent<EnemyProjectileAttack>();
            _enemySelfExplodeState = statesLayer.GetComponent<EnemySelfExplode>();
            
            _enemyFlyTowardsTargetState.OnStateFinished += ChangeToEnemyAttackState;
            _enemyProjectileAttackState.OnStateFinished += ChangeToFlyingTowardsState;
        }

        protected override void Tick()
        {
            if (currentTarget != null && !_changedStateToFlyTowardsTarget)
            {
                base.ChangeState(_enemyFlyTowardsTargetState, base.currentTarget);
                _changedStateToFlyTowardsTarget = true;
            }

            // Meaning we check if we're attacking a final target
            if (!_attackingFinalTarget && currentTarget != null && ((1 << currentTarget.gameObject.layer) & finalEnemyLayer) != 0)
            {
                _attackingFinalTarget = true;
            }
            
            // Final target has been destroyed
            if (_attackingFinalTarget && currentTarget == null)
            {
                base.ChangeState(_enemySelfExplodeState);
            }
        }
        
        protected override void FixedTick()
        {
        }

        protected override void Deinitialize()
        {
            _changedStateToFlyTowardsTarget = false;
            _attackingFinalTarget = false;
            _enemyFlyTowardsTargetState.OnStateFinished -= ChangeToEnemyAttackState;
            _enemyProjectileAttackState.OnStateFinished -= ChangeToFlyingTowardsState;
        }
        
        private void ChangeToEnemyAttackState()
        {
            base.ChangeState(_enemyProjectileAttackState, base.currentTarget);
        }
        private void ChangeToFlyingTowardsState()
        {
            base.ChangeState(_enemyFlyTowardsTargetState, base.currentTarget);
        }
    }
}