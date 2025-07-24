using Core.Enemy.States;
using Core.HealthSystem;
using UnityEngine;

namespace Core.Enemy.Types
{
    public class HeavyBomber: FlyingEnemy
    {
        [SerializeField] private GameObject statesLayer;

        private Transform _currentTarget;
        
        private EnemyFindTarget _findTargetState;
        private EnemyFlyTowardsTarget _flyTowardsTargetState;
        private EnemyAttack _enemyAttackState;
            
        protected override void Initialize()
        {
            _findTargetState = statesLayer.GetComponent<EnemyFindTarget>();
            _flyTowardsTargetState = statesLayer.GetComponent<EnemyFlyTowardsTarget>();
            _enemyAttackState = statesLayer.GetComponent<EnemyAttack>();
            
            base.ChangeState(_findTargetState);

            _findTargetState.OnTargetFound += ChangeToFlyTowardsEnemyState;
            _flyTowardsTargetState.OnStateFinished += ChangeToEnemyAttackState;
        }
        
        protected override void Tick()
        {
            if (!_findTargetState.IsStateActive)
            {
                _findTargetState.Tick();
            }

            if (_findTargetState.IsStateActive && !_flyTowardsTargetState.IsStateActive)
            {
                _flyTowardsTargetState.Tick();
            }

            if (_flyTowardsTargetState.IsStateActive && _findTargetState.IsStateActive)
            {
                _enemyAttackState.Tick();
            }
        }

        protected override void FixedTick()
        {
        }

        protected override void Deinitialize()
        {
            _findTargetState.OnTargetFound -= ChangeToFlyTowardsEnemyState;
        }
        
        private void ChangeToFlyTowardsEnemyState(Transform targetTransform)
        {
            _findTargetState.OnTargetFound -= ChangeToFlyTowardsEnemyState;
            
            base.ChangeState(_flyTowardsTargetState);
            _currentTarget = targetTransform;
            _flyTowardsTargetState.SetTarget(_currentTarget);
        }
        
        private void ChangeToEnemyAttackState()
        {
            _flyTowardsTargetState.OnStateFinished -= ChangeToEnemyAttackState;
            
            base.ChangeState(_enemyAttackState);
            _enemyAttackState.SetTarget(_currentTarget);
        }

    }
}