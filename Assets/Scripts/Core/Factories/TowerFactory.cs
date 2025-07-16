using Core.TowersBehaviour;
using UnityEngine;

namespace Core.Factories
{
    public abstract class TowerFactory: MonoBehaviour
    {
       public abstract Tower CreateTower(Vector3 position, Quaternion rotation); 
    }
}