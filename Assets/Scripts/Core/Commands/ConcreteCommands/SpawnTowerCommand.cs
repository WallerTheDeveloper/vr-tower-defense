using System;
using Core.Factories;
using UnityEngine;

namespace Core.Commands.ConcreteCommands
{
    [Serializable]
    public class SpawnTowerCommand : ICommand
    {
        private UnitFactory _unitFactory;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        
        public SpawnTowerCommand(UnitFactory unitFactory, Vector3 position, Quaternion rotation)
        {
            _unitFactory = unitFactory;
            _spawnPosition = position;
            _spawnRotation = rotation;
        }
    
        public void Execute()
        {
            _unitFactory.CreateTower(_spawnPosition, _spawnRotation);
        }
    
        public void Undo()
        {
        }
    }
}