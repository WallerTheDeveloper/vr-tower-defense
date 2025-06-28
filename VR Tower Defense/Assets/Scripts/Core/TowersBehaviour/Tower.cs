using UnityEngine;
using System.Collections;
using Core.TowersBehaviour.States;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace Core.TowersBehaviour
{
    public abstract class Tower : MonoBehaviour
    {
        private TowerAutoOrient _autoOrient;
        public bool IsActive { get; private set; } = false;

        private void Awake()
        {
            _autoOrient = GetComponent<TowerAutoOrient>();
        }

        private void OnEnable()
        {
            if (_autoOrient != null)
            {
                _autoOrient.OnPlacementComplete += Activate;
            }
        }

        private void OnDisable()
        {
            if (_autoOrient != null)
            {
                _autoOrient.OnPlacementComplete -= Activate;
            }
        }
        
        private void Update()
        {
            if (IsActive)
            {
                HandleBehaviour();
            }
        }
        
        public abstract void HandleBehaviour();

        private void Activate()
        {
            IsActive = true;
            Debug.Log($"{gameObject.name} has been placed and is now ACTIVE.");

            var grabbableObject = GetComponent<XRGrabInteractable>();
            if (grabbableObject != null)
            {
                grabbableObject.enabled = false;
            }
        }
    }
}