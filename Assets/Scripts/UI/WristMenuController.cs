using UnityEngine;
using UnityEngine.XR.Hands;
using System.Collections;

namespace UI
{
    public class WristMenuController : MonoBehaviour
    {
        [Header("Menu Settings")]
        [SerializeField] private CanvasGroup menuCanvasGroup;
        [SerializeField] private Transform wristMenuTransform;
        [SerializeField] private Vector3 wristMenuPositionThreshold;
        [Range(-1f, 1f)]
        [SerializeField] private float palmDotThreshold = 0.2f;
        [Range(-1f, 1f)]
        [SerializeField] private float palmDotHideThreshold = 0.0f;
        [SerializeField] private float animationSpeed = 3f;
        
        [Header("Hand Tracking")]
        [SerializeField] private XRHandTrackingEvents handTrackingEvents;
        [SerializeField] private bool useLeftHand = true;
        
        private Transform cameraTransform;
        private bool isMenuVisible = false;
        private bool isHandTracked = false;
        private Vector3 wristPosition;
        private Quaternion wristRotation;
        private Vector3 palmDirection;
        private Coroutine fadeCoroutine;
        
        private XRHandSubsystem handSubsystem;
        
        private void Start()
        {
            cameraTransform = Camera.main.transform;
            
            if (menuCanvasGroup != null)
            {
                menuCanvasGroup.alpha = 0f;
                menuCanvasGroup.interactable = false;
                menuCanvasGroup.blocksRaycasts = false;
            }
            
            InitializeHandTracking();
            
            if (handTrackingEvents != null)
            {
                if (useLeftHand)
                {
                    handTrackingEvents.jointsUpdated.AddListener(OnLeftHandJointsUpdated);
                    handTrackingEvents.trackingAcquired.AddListener(OnLeftHandTrackingAcquired);
                    handTrackingEvents.trackingLost.AddListener(OnLeftHandTrackingLost);
                }
                else
                {
                    handTrackingEvents.jointsUpdated.AddListener(OnRightHandJointsUpdated);
                    handTrackingEvents.trackingAcquired.AddListener(OnRightHandTrackingAcquired);
                    handTrackingEvents.trackingLost.AddListener(OnRightHandTrackingLost);
                }
            }
        }
        
        private void InitializeHandTracking()
        {
            var handSubsystems = new System.Collections.Generic.List<XRHandSubsystem>();
            SubsystemManager.GetSubsystems(handSubsystems);
            
            if (handSubsystems.Count > 0)
            {
                handSubsystem = handSubsystems[0];
                Debug.Log("Hand tracking subsystem found and initialized");
            }
            else
            {
                Debug.LogWarning("No hand tracking subsystem found.");
            }
        }
        
        private void Update()
        {
            UpdateHandPosition();
            CheckMenuVisibility();
            UpdateMenuPositionAndOrientation();
        }
        
        private void UpdateHandPosition()
        {
            if (isHandTracked && handSubsystem != null)
            {
                UpdateFromHandTracking();
            }
        }
        
        private void UpdateFromHandTracking()
        {
            if (handSubsystem != null && handSubsystem.running)
            {
                XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
                
                if (hand.isTracked)
                {
                    if (hand.GetJoint(XRHandJointID.Wrist).TryGetPose(out Pose wristPose))
                    {
                        wristPosition = wristPose.position;
                        wristRotation = wristPose.rotation;
                        
                        if (hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palmPose))
                        {
                            palmDirection = palmPose.rotation * Vector3.down;
                        }
                    }
                }
            }
        }
        
        private void CheckMenuVisibility()
        {
            bool shouldShowMenu = ShouldShowMenu();
            
            if (shouldShowMenu && !isMenuVisible)
            {
                ShowMenu();
            }
            else if (!shouldShowMenu && isMenuVisible)
            {
                HideMenu();
            }
        }
        
        private bool ShouldShowMenu()
        {
            if (!isHandTracked || palmDirection == Vector3.zero)
                return false;
                
            Vector3 headPosition = cameraTransform.position;
            Vector3 palmToHead = (headPosition - wristPosition).normalized;
            
            float palmDot = Vector3.Dot(palmDirection, palmToHead);
            
            float dotThreshold = isMenuVisible ? palmDotHideThreshold : palmDotThreshold;
            
            return palmDot >= dotThreshold;
        }
        
        private void ShowMenu()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            
            fadeCoroutine = StartCoroutine(FadeMenu(true));
            isMenuVisible = true;
        }
        
        private void HideMenu()
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            
            fadeCoroutine = StartCoroutine(FadeMenu(false));
            isMenuVisible = false;
        }
        
        private IEnumerator FadeMenu(bool fadeIn)
        {
            if (menuCanvasGroup == null)
                yield break;
            
            float startAlpha = menuCanvasGroup.alpha;
            float targetAlpha = fadeIn ? 1f : 0f;
            float elapsedTime = 0f;
            
            float duration = Mathf.Abs(targetAlpha - startAlpha) / animationSpeed;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                progress = Mathf.SmoothStep(0f, 1f, progress);
                
                menuCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
                yield return null;
            }
            
            menuCanvasGroup.alpha = targetAlpha;
            menuCanvasGroup.interactable = fadeIn;
            menuCanvasGroup.blocksRaycasts = fadeIn;
        }
        
        private void UpdateMenuPositionAndOrientation()
        {
            if (wristMenuTransform != null && cameraTransform != null && isHandTracked &&
                (isMenuVisible || menuCanvasGroup.alpha > 0.01f))
            {
                Vector3 localOffset = wristMenuPositionThreshold;
                
                Vector3 worldOffset = wristRotation * localOffset;
                Vector3 targetPosition = wristPosition + worldOffset;
                
                wristMenuTransform.position = Vector3.Lerp(
                    wristMenuTransform.position, 
                    targetPosition, 
                    Time.deltaTime * 10f
                );
                
                Vector3 toCamera = cameraTransform.position - wristMenuTransform.position;
                
                if (toCamera.sqrMagnitude > 0.0001f)
                {
                    Vector3 upVector = cameraTransform.up;
                    
                    Quaternion targetRotation = Quaternion.LookRotation(-toCamera.normalized, upVector);
                    
                    wristMenuTransform.rotation = Quaternion.Slerp(
                        wristMenuTransform.rotation, 
                        targetRotation, 
                        Time.deltaTime * 10f
                    );
                }
                else
                {
                    wristMenuTransform.rotation = Quaternion.Slerp(
                        wristMenuTransform.rotation, 
                        cameraTransform.rotation, 
                        Time.deltaTime * 10f
                    );
                }
            }
        }
        
        private void OnLeftHandJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            if (useLeftHand)
            {
                isHandTracked = true;
            }
        }
        
        private void OnRightHandJointsUpdated(XRHandJointsUpdatedEventArgs args)
        {
            if (!useLeftHand)
            {
                isHandTracked = true;
            }
        }
        
        private void OnLeftHandTrackingAcquired()
        {
            if (useLeftHand)
            {
                isHandTracked = true;
            }
        }
        
        private void OnRightHandTrackingAcquired()
        {
            if (!useLeftHand)
            {
                isHandTracked = true;
            }
        }
        
        private void OnLeftHandTrackingLost()
        {
            if (useLeftHand)
            {
                isHandTracked = false;
            }
        }
        
        private void OnRightHandTrackingLost()
        {
            if (!useLeftHand)
            {
                isHandTracked = false;
            }
        }
        
        private void OnDestroy()
        {
            if (handTrackingEvents != null)
            {
                handTrackingEvents.jointsUpdated.RemoveListener(OnLeftHandJointsUpdated);
                handTrackingEvents.jointsUpdated.RemoveListener(OnRightHandJointsUpdated);
                handTrackingEvents.trackingAcquired.RemoveListener(OnLeftHandTrackingAcquired);
                handTrackingEvents.trackingAcquired.RemoveListener(OnRightHandTrackingAcquired);
                handTrackingEvents.trackingLost.RemoveListener(OnLeftHandTrackingLost);
                handTrackingEvents.trackingLost.RemoveListener(OnRightHandTrackingLost);
            }
        }
        
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || !isHandTracked)
                return;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(wristPosition, 0.02f);
            
            if (wristRotation != Quaternion.identity)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(wristPosition, wristRotation * Vector3.right * 0.05f);
                Gizmos.color = Color.green;
                Gizmos.DrawRay(wristPosition, wristRotation * Vector3.up * 0.05f);
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(wristPosition, wristRotation * Vector3.forward * 0.05f);
                
                Vector3 localOffset = wristMenuPositionThreshold;
                Vector3 worldOffset = wristRotation * localOffset;
                Vector3 targetPosition = wristPosition + worldOffset;
                
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(wristPosition, targetPosition);
                Gizmos.DrawWireSphere(targetPosition, 0.015f);
            }
            
            if (palmDirection != Vector3.zero && cameraTransform != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(wristPosition, palmDirection * 0.15f);
                
                Vector3 palmToHead = (cameraTransform.position - wristPosition).normalized;
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(wristPosition, palmToHead * 0.1f);
                
                float palmDot = Vector3.Dot(palmDirection, palmToHead);
                Gizmos.color = palmDot >= palmDotThreshold ? Color.green : Color.red;
                Gizmos.DrawWireSphere(wristPosition + Vector3.up * 0.05f, 0.01f);
                
                #if UNITY_EDITOR
                UnityEditor.Handles.Label(wristPosition + Vector3.up * 0.1f, $"Dot: {palmDot:F2}");
                UnityEditor.Handles.Label(wristPosition + Vector3.up * 0.12f, $"Visible: {isMenuVisible}");
                #endif
            }
        }
    }
}