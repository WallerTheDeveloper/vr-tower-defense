using Core.TowersBehaviour;
using UnityEngine;

namespace Core.Factories
{
    public abstract class UnitFactory: MonoBehaviour
    {
       public abstract Unit CreateTower(Vector3 position, Quaternion rotation); 
    }
}