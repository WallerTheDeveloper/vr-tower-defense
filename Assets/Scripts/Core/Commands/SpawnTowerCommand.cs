using System;
using Core.Factories;
using UnityEngine;

namespace Core.Commands
{
    [Serializable]
    public class SpawnTowerCommand : ICommand
    {
        private TowerType _towerType;
        private TowerFactory _towerFactory;
        private Vector3 _spawnPosition;
        private Quaternion _spawnRotation;
        
        public SpawnTowerCommand(TowerFactory towerFactory, TowerType type, Vector3 position, Quaternion rotation)
        {
            _towerType = type;
            _towerFactory = towerFactory;
            _spawnPosition = position;
            _spawnRotation = rotation;
        }
    
        public void Execute()
        {
            _towerFactory.CreateTower(_towerType, _spawnPosition, _spawnRotation);
        }
    
        public void Undo()
        {
        }
    }
}