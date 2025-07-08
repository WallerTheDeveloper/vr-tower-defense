using UnityEngine;

namespace Core.Data
{
    [CreateAssetMenu(fileName = "Flying Enemy Settings", menuName = "Data/FlyingEnemySettings", order = 1)]
    public class FlyingEnemySettings : ScriptableObject
    {
        public float MaxHealth;
        public float Damage;
        public ParticleSystem DeathParticleSystem;
        public float DeathParticleSystemLifetime;
    }
}