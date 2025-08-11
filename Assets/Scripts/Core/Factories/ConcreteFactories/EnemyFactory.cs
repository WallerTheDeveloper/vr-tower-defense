using Core.Pooling;
using UnityEngine;

namespace Core.Factories.ConcreteFactories
{
    public class EnemyFactory : UnitFactory
    {
        [SerializeField] private Unit unit;
        public override Unit CreateTower(Vector3 position, Quaternion rotation)
        {
            Unit createdUnit = ObjectPoolManager.SpawnObject(unit, position, rotation);
            return createdUnit;
        }
    }
}