using Data;
using UnityEngine;

namespace Core.Factories
{
    public class RocketFactory : ProjectileFactory
    {
        [SerializeField] private Projectile projectile;
        public override Projectile CreateProjectile(BaseUnitSettings unitSettings, Vector3 position, Quaternion rotation)
        {
            Projectile createdProjectile = null; 
            createdProjectile = Instantiate(projectile, position, rotation);
            createdProjectile.Initialize(unitSettings);
            return createdProjectile;
        }
    }
}