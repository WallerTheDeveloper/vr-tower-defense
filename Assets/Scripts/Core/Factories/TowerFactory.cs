using Core.Commands;
using Core.TowersBehaviour;
using UnityEngine;

namespace Core.Factories
{
    public class TowerFactory: MonoBehaviour
    {
        [SerializeField] private Tower tower;
        public Tower CreateTower(TowerType type, Vector3 position, Quaternion rotation)
        {
            Tower createdTower = null;
            if (type == TowerType.MachineGun)
            {
                createdTower = Instantiate(tower, position, rotation);
            }

            return createdTower;
        }
    }
}