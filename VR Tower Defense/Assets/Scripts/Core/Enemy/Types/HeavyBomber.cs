using Core.Enemy.States;
using UnityEngine;

namespace Core.Enemy.Types
{
    public class HeavyBomber: FlyingEnemy
    {
        [SerializeField] private GameObject statesLayer;
        
        private EnemyFindTarget _findTargetState;
        private EnemyFlyTowardsTarget _flyTowardsTargetState;
        protected override void Initialize()
        {
            _findTargetState = statesLayer.GetComponent<EnemyFindTarget>();
            _flyTowardsTargetState = statesLayer.GetComponent<EnemyFlyTowardsTarget>();
            
            base.ChangeState(_findTargetState);

            _findTargetState.OnTargetFound += ChangeToFlyTowardsEnemyState;
        }
        
        protected override void Tick()
        {
            if (!_findTargetState.IsStateFinished)
            {
                _findTargetState.Tick();
            }

            if (_findTargetState.IsStateFinished)
            {
                _flyTowardsTargetState.Tick();
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
            _flyTowardsTargetState.SetTarget(targetTransform);
        }
    }
}