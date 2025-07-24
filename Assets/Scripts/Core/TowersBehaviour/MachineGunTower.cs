using Core.Factories;
using Core.TowersBehaviour.States;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowersBehaviour
{
    public class MachineGunTower : Tower
    {
        [SerializeField] private GameObject statesLayer;
        
        protected override void Initialize()
        {
        }

        protected override void Tick()
        {
        }

        protected override void FixedTick()
        {
        }
        
        protected override void Deinitialize()
        {
        }
    }
}