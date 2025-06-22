using UnityEngine;

namespace Core.TowersBehaviour
{
    public class MachineGunTower : Tower
    {
        [SerializeField] private GameObject projectile;
        [SerializeField] private GameObject towerObject;
        [SerializeField] private GameObject enemy;
        [SerializeField] private float anglePerSecond;
        public override void HandleBehaviour()
        {
            RotateTowardsTarget();
        }

        private void RotateTowardsTarget()
        {
             var targetRotation = Quaternion.LookRotation(enemy.transform.position - towerObject.transform.position);
             towerObject.transform.rotation = Quaternion.RotateTowards(towerObject.transform.rotation, targetRotation, anglePerSecond * Time.deltaTime);
        }
    }
}