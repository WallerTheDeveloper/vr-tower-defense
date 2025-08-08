using UnityEngine;

namespace Core.Factories
{
    public class EnemyFactory : UnitFactory
    {
        [SerializeField] private Unit unit;
        public override Unit CreateTower(Vector3 position, Quaternion rotation)
        {
            Unit createdUnit = null;
            createdUnit = Instantiate(unit, position, rotation);

            return createdUnit;
        }
    }
}