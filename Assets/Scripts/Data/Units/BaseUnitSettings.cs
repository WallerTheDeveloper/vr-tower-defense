using UnityEngine;

namespace Data.Units
{
    public abstract class BaseUnitSettings : ScriptableObject
    {
        public float MaxHealth;
        public float Damage;
        public ParticleSystem DeathParticleSystem;
        public float DeathParticleSystemLifetime;
    }
}