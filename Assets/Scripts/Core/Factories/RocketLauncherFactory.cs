using Core.TowersBehaviour;
using UnityEngine;

namespace Core.Factories
{
    public class RocketLauncherFactory : TowerFactory
    {
        [SerializeField] private Tower tower;
        public override Tower CreateTower(Vector3 position, Quaternion rotation)
        {
            Tower createdTower = null;
            createdTower = Instantiate(tower, position, rotation);

            return createdTower;
        }
    }
}