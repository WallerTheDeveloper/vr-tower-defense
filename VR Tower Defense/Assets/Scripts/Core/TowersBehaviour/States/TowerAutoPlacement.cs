using UnityEngine;
using System;
using Core.StateMachine;
using UnityEngine.Serialization;

namespace Core.TowersBehaviour.States
{
    public class TowerAutoPlacement : MonoBehaviour, IState
    {
        public event Action OnStateFinished;
        
        [Header("Auto-Orient Settings")]
        [SerializeField] private LayerMask groundLayerMask = 1;
        [SerializeField] private float orientationSpeed = 5f;
        [SerializeField] private float stabilityThreshold = 0.1f;
        [SerializeField] private bool useGroundNormal = true;
        [SerializeField] private bool snapToGround = true;
        [SerializeField] private float groundSnapDistance = 0.5f;
        
        [Header("Tower Base Settings")]
        [SerializeField] private Transform towerBase;
        [SerializeField] private Vector3 baseOffset = Vector3.zero; 
        
        [SerializeField] private Rigidbody _rigidbody;
        
        private bool isLanded = false;
        private bool isOrienting = false;
        private Vector3 targetGroundNormal;
        private Vector3 targetGroundPosition;
        private Quaternion targetRotation;
        
        public void Enter()
        {
            if (towerBase == null)
                towerBase = transform;
        }

        public void Tick() {}

        public void FixedTick()
        {
            if (!isLanded && ShouldCheckForLanding())
            {
                CheckForGroundContact();
            }
            
            if (isOrienting)
            {
                PerformOrientation();
            }
        }

        public void Exit() {}
        
        private void OnCollisionEnter(Collision collision)
        {
            if (!isLanded && IsGroundLayer(collision.gameObject.layer))
            {
                ContactPoint bestContact = GetBestContactPoint(collision.contacts);
                
                RaycastHit hit;
                if (Physics.Raycast(bestContact.point + Vector3.up * 0.1f, Vector3.down, out hit, 1f, groundLayerMask))
                {
                    StartLandingSequence(hit);
                }
            }
        }
        
        private bool ShouldCheckForLanding()
        {
            return _rigidbody.linearVelocity.magnitude < stabilityThreshold || _rigidbody.linearVelocity.y < 0;
        }
        
        private void CheckForGroundContact()
        {
            Vector3 castOrigin = GetBasePosition();
            
            RaycastHit hit;
            if (Physics.Raycast(castOrigin, Vector3.down, out hit, groundSnapDistance, groundLayerMask))
            {
                StartLandingSequence(hit);
            }
        }
        
        private void StartLandingSequence(RaycastHit groundHit)
        {
            if (isLanded) return;
            
            isLanded = true;
            isOrienting = true;
            
            targetGroundNormal = groundHit.normal;
            targetGroundPosition = groundHit.point;
            
            if (useGroundNormal)
            {
                targetRotation = Quaternion.FromToRotation(transform.up, targetGroundNormal) * transform.rotation;
            }
            else
            {
                targetRotation = Quaternion.LookRotation(transform.forward, Vector3.up);
            }
            
            _rigidbody.linearDamping = 5f;
            _rigidbody.angularDamping = 5f;
        }
        
        private void PerformOrientation()
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, orientationSpeed * Time.fixedDeltaTime);
            
            if (snapToGround)
            {
                Vector3 desiredPosition = targetGroundPosition - GetLocalBaseOffset();
                transform.position = Vector3.Lerp(transform.position, desiredPosition, orientationSpeed * Time.fixedDeltaTime);
            }
            
            float rotationDifference = Quaternion.Angle(transform.rotation, targetRotation);
            
            if (rotationDifference < 1f)
            {
                CompleteOrientation();
            }
        }
        
        private void CompleteOrientation()
        {
            isOrienting = false;
            
            transform.rotation = targetRotation;
            
            if (snapToGround)
            {
                transform.position = targetGroundPosition - GetLocalBaseOffset();
            }
            
            _rigidbody.isKinematic = true;
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;

            this.enabled = false;

            OnStateFinished?.Invoke();
        }
        
        private Vector3 GetBasePosition()
        {
            if (towerBase != null)
                return towerBase.position;
            
            return transform.position + GetLocalBaseOffset();
        }
        
        private Vector3 GetLocalBaseOffset()
        {
            return transform.TransformDirection(baseOffset);
        }
        
        private ContactPoint GetBestContactPoint(ContactPoint[] contacts)
        {
            Vector3 basePos = GetBasePosition();
            ContactPoint bestContact = contacts[0];
            float closestDistance = Vector3.Distance(basePos, contacts[0].point);
            
            for (int i = 1; i < contacts.Length; i++)
            {
                float distance = Vector3.Distance(basePos, contacts[i].point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestContact = contacts[i];
                }
            }
            
            return bestContact;
        }
        
        private bool IsGroundLayer(int layer)
        {
            return (groundLayerMask.value & (1 << layer)) != 0;
        }
    }
}