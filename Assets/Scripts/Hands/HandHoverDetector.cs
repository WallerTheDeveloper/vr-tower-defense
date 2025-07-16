using UnityEngine;
using UnityEngine.XR.Hands;

namespace Hands
{
    public class HandHoverDetector : MonoBehaviour
    {
        [Header("Hand Tracking")]
        [SerializeField] private bool useLeftHand = true;
        [SerializeField] private XRHandJointID trackingJoint = XRHandJointID.IndexTip;
        
        private XRHandSubsystem handSubsystem;
        private Vector3 currentHandPosition;
        private bool isHandTracked = false;
        
        public Vector3 HandPosition => currentHandPosition;
        public bool IsHandTracked => isHandTracked;
        
        private void Start()
        {
            InitializeHandTracking();
        }
        
        private void InitializeHandTracking()
        {
            var handSubsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(handSubsystems);
            
            if (handSubsystems.Count > 0)
            {
                handSubsystem = handSubsystems[0];
            }
        }
        
        private void Update()
        {
            UpdateHandPosition();
        }
        
        private void UpdateHandPosition()
        {
            if (handSubsystem == null || !handSubsystem.running)
            {
                isHandTracked = false;
                return;
            }
            
            XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
            
            if (hand.isTracked && hand.GetJoint(trackingJoint).TryGetPose(out Pose jointPose))
            {
                currentHandPosition = jointPose.position;
                isHandTracked = true;
            }
            else
            {
                isHandTracked = false;
            }
        }
        
        public bool IsHoveringOverPoint(Vector3 point, float distance)
        {
            if (!isHandTracked) return false;
            return Vector3.Distance(currentHandPosition, point) <= distance;
        }
        
        public float GetDistanceToPoint(Vector3 point)
        {
            if (!isHandTracked) return float.MaxValue;
            return Vector3.Distance(currentHandPosition, point);
        }
    }
}