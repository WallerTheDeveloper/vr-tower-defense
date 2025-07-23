using System;
using Core.Factories;
using UnityEngine;

namespace Core.Commands
{
    [Serializable]
    public class SpawnTowerCommand : ICommand
    {
        private TowerFactory _towerFactory;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        
        public SpawnTowerCommand(TowerFactory towerFactory, Vector3 position, Quaternion rotation)
        {
            _towerFactory = towerFactory;
            _spawnPosition = position;
            _spawnRotation = rotation;
        }
    
        public void Execute()
        {
            _towerFactory.CreateTower(_spawnPosition, _spawnRotation);
        }
    
        public void Undo()
        {
        }
    }
}