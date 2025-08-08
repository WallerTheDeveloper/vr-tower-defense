using Data;
using UnityEngine;

namespace Core.Factories
{
    public abstract class ProjectileFactory : MonoBehaviour
    {
        public abstract Projectile CreateProjectile(BaseUnitSettings unitSettings, Vector3 position, Quaternion rotation);
    }
}