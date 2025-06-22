using System;
using System.Collections;
using Core.TowersBehaviour.States;
using UnityEngine;

namespace Core.TowersBehaviour
{
    public abstract class Tower : MonoBehaviour
    {
        private TowerAutoOrient _autoOrient;
        private bool _isReadyForActivation = false;

        public bool IsActive { get; private set; } = false;

        private void Start()
        {
            _autoOrient = GetComponent<TowerAutoOrient>();
            StartCoroutine(WaitForPlacement());
        }

        private void Update()
        {
            if (IsActive)
            {
                HandleBehaviour();
            }
        }

        public abstract void HandleBehaviour();
        
        private IEnumerator WaitForPlacement()
        {
            while (_autoOrient != null && !_autoOrient.IsPlaced())
            {
                yield return null;
            }
            
            IsActive = true;
        }
    }
}