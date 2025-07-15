using System.Collections;
using Core.Commands;
using Core.Factories;
using Hands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UI
{
    public class TowerMenuButton : MonoBehaviour
    {
        [Header("Tower Settings")]
        [SerializeField] private TowerType towerType;
        [SerializeField] private TowerFactory towerFactory;
        
        [Header("Hover Detection")]
        [SerializeField] private float hoverDistance = 0.05f;
        [SerializeField] private bool useLeftHand = true;
        
        [Header("Pinch Detection")]
        [SerializeField] private float pinchThreshold = 0.03f;
        [SerializeField] private float pinchReleaseThreshold = 0.05f;
        [SerializeField] private bool requireHoverToPinch = true;
        [SerializeField] private bool usePinchReleaseInteraction = false;
        
        [Header("Tower Spawning")]
        [SerializeField] private bool spawnAtPinchLocation = true;
        [SerializeField] private bool autoGrabOnSpawn = true;
        
        [Header("Position Configuration")]
        [SerializeField] private bool useManualCenter = false;
        [SerializeField] private Vector3 manualButtonCenter = Vector3.zero;
        [SerializeField] private Transform centerTransform;
        [SerializeField] private Vector3 centerOffset = Vector3.zero;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color pinchingColor = Color.orange;
        [SerializeField] private Color selectedColor = Color.green;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip pinchStartSound;
        [SerializeField] private AudioClip pinchReleaseSound;
        
        [Header("Events")]
        public UnityEvent OnHoverEnter;
        public UnityEvent OnHoverExit;
        public UnityEvent OnPinchStart;
        public UnityEvent OnPinchEnd;
        public UnityEvent OnPinchSelect;
        
        private bool isHovering = false;
        private bool wasHovering = false;
        private bool isPinching = false;
        private bool wasPinching = false;
        private bool hasTriggeredPinchAction = false;
        private Vector3 buttonCenter;
        private Vector3 lastPinchPosition;
        private Button button;
        private AudioSource audioSource;
        private XRHandSubsystem handSubsystem;
        private float lastHandDistance = float.MaxValue;
        private float lastPinchDistance = float.MaxValue;
        
        private HandHoverDetector handHoverDetector;
        
        private void Start()
        {
            button = GetComponent<Button>();
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            InitializeHandTracking();
            
            handHoverDetector = FindFirstObjectByType<HandHoverDetector>();
            
            UpdateVisualState(ButtonState.Normal);
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
            UpdateButtonCenter();
            CheckHandHover();
            CheckPinchGesture();
            HandleInteractionLogic();
        }
        
        private void UpdateButtonCenter()
        {
            if (useManualCenter)
            {
                buttonCenter = manualButtonCenter;
                return;
            }
            
            if (centerTransform != null)
            {
                buttonCenter = centerTransform.position + centerOffset;
                return;
            }
            
            Vector3 calculatedCenter;
            
            if (GetComponent<RectTransform>() != null)
            {
                RectTransform rectTransform = GetComponent<RectTransform>();
                Canvas canvas = GetComponentInParent<Canvas>();
                
                if (canvas != null)
                {
                    if (canvas.renderMode == RenderMode.WorldSpace)
                    {
                        calculatedCenter = rectTransform.TransformPoint(Vector3.zero);
                    }
                    else
                    {
                        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, rectTransform.position);
                        calculatedCenter = canvas.worldCamera != null ? 
                            canvas.worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, canvas.planeDistance)) :
                            Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 10f));
                    }
                }
                else
                {
                    calculatedCenter = rectTransform.position;
                }
            }
            else
            {
                calculatedCenter = transform.position;
            }
            
            buttonCenter = calculatedCenter + centerOffset;
        }
        
        private void CheckHandHover()
        {
            if (handSubsystem == null || !handSubsystem.running)
            {
                isHovering = false;
                lastHandDistance = float.MaxValue;
                return;
            }
            
            XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
            
            if (!hand.isTracked)
            {
                isHovering = false;
                lastHandDistance = float.MaxValue;
                return;
            }
            
            if (hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTipPose))
            {
                float distance = Vector3.Distance(indexTipPose.position, buttonCenter);
                lastHandDistance = distance;
                
                bool withinDistance = distance <= hoverDistance;
                
                if (withinDistance)
                {
                    isHovering = PieMenuHoverManager.Instance.RequestHover(this, distance);
                }
                else
                {
                    isHovering = false;
                    PieMenuHoverManager.Instance.ReleaseHover(this);
                }
            }
            else
            {
                isHovering = false;
                lastHandDistance = float.MaxValue;
                PieMenuHoverManager.Instance.ReleaseHover(this);
            }
        }
        
        private void CheckPinchGesture()
        {
            if (handSubsystem == null || !handSubsystem.running)
            {
                isPinching = false;
                lastPinchDistance = float.MaxValue;
                return;
            }
            
            XRHand hand = useLeftHand ? handSubsystem.leftHand : handSubsystem.rightHand;
            
            if (!hand.isTracked)
            {
                isPinching = false;
                lastPinchDistance = float.MaxValue;
                return;
            }
            
            if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumbTipPose) &&
                hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose indexTipPose))
            {
                float pinchDistance = Vector3.Distance(thumbTipPose.position, indexTipPose.position);
                lastPinchDistance = pinchDistance;
                
                lastPinchPosition = (thumbTipPose.position + indexTipPose.position) * 0.5f;
                
                if (!isPinching && pinchDistance <= pinchThreshold && !PieMenuHoverManager.Instance.HasAnyButtonTriggeredPinchAction())
                {
                    bool canStartPinch = false;
                    
                    if (requireHoverToPinch)
                    {
                        canStartPinch = isHovering;
                    }
                    else
                    {
                        canStartPinch = isHovering || IsClosestButtonToPinch(thumbTipPose.position, indexTipPose.position);
                    }
                    
                    if (canStartPinch)
                    {
                        isPinching = true;
                    }
                }
                else if (isPinching && pinchDistance >= pinchReleaseThreshold)
                {
                    isPinching = false;
                    if (hasTriggeredPinchAction)
                    {
                        hasTriggeredPinchAction = false;
                        PieMenuHoverManager.Instance.ClearPinchActionTrigger();
                    }
                }
            }
            else
            {
                isPinching = false;
                lastPinchDistance = float.MaxValue;
                lastPinchPosition = Vector3.zero;
            }
        }
        
        private bool IsClosestButtonToPinch(Vector3 thumbPos, Vector3 indexPos)
        {
            Vector3 pinchCenter = (thumbPos + indexPos) * 0.5f;
            
            float distanceToPinchCenter = Vector3.Distance(pinchCenter, buttonCenter);
            
            if (distanceToPinchCenter > hoverDistance)
            {
                return false;
            }
            
            return PieMenuHoverManager.Instance.RequestPinch(this, distanceToPinchCenter);
        }
        
        private void HandleInteractionLogic()
        {
            if (isHovering && !wasHovering)
            {
                OnHoverEnterAction();
            }
            else if (!isHovering && wasHovering)
            {
                OnHoverExitAction();
            }
            
            if (isPinching && !wasPinching)
            {
                OnPinchStartAction();
            }
            else if (!isPinching && wasPinching && usePinchReleaseInteraction)
            {
                OnPinchEndAction();
            }
            
            wasHovering = isHovering;
            wasPinching = isPinching;
        }
        
        public float GetHandDistance()
        {
            return lastHandDistance;
        }
        
        public void ForceHoverExit()
        {
            if (isHovering)
            {
                isHovering = false;
                OnHoverExitAction();
            }
        }
        
        public void ForcePinchExit()
        {
            if (isPinching)
            {
                isPinching = false;
                OnPinchEndAction();
            }
        }
        
        public bool HasTriggeredPinchAction()
        {
            return hasTriggeredPinchAction;
        }
        
        public void ResetPinchActionTrigger()
        {
            hasTriggeredPinchAction = false;
        }
        
        private void OnHoverEnterAction()
        {
            UpdateVisualState(isPinching ? ButtonState.Pinching : ButtonState.Hover);
            
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
            
            OnHoverEnter?.Invoke();
        }
        
        private void OnHoverExitAction()
        {
            UpdateVisualState(ButtonState.Normal);
            
            OnHoverExit?.Invoke();
        }
        
        private void OnPinchStartAction()
        {
            UpdateVisualState(ButtonState.Pinching);
            
            if (pinchStartSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pinchStartSound);
            }
            
            OnPinchStart?.Invoke();
            
            if ((isHovering || !requireHoverToPinch) && !hasTriggeredPinchAction && !PieMenuHoverManager.Instance.HasAnyButtonTriggeredPinchAction())
            {
                hasTriggeredPinchAction = true;
                PieMenuHoverManager.Instance.SetPinchActionTriggered(this);
                OnPinchSelectAction();
                PieMenuHoverManager.Instance.ReleasePinch(this);
                UpdateVisualState(isHovering ? ButtonState.Hover : ButtonState.Normal);
            }
        }
        
        private void OnPinchEndAction()
        {
            PieMenuHoverManager.Instance.ReleasePinch(this);
            
            UpdateVisualState(isHovering ? ButtonState.Hover : ButtonState.Normal);
            
            if (pinchReleaseSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(pinchReleaseSound);
            }
            
            OnPinchEnd?.Invoke();
        }
        
        private void OnPinchSelectAction()
        {
            UpdateVisualState(ButtonState.Selected);
            
            OnPinchSelect?.Invoke();
            
            OnButtonClick();
            
            StartCoroutine(ResetVisualStateAfterDelay(0.2f));
        }
        
        private IEnumerator ResetVisualStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateVisualState(isHovering ? ButtonState.Hover : ButtonState.Normal);
        }
        
        private void UpdateVisualState(ButtonState state)
        {
            if (button == null) return;
            
            Color targetColor = state switch
            {
                ButtonState.Normal => normalColor,
                ButtonState.Hover => hoverColor,
                ButtonState.Pinching => pinchingColor,
                ButtonState.Selected => selectedColor,
                _ => normalColor
            };

            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = targetColor;
            colorBlock.highlightedColor = targetColor;
            colorBlock.selectedColor = targetColor;
            button.colors = colorBlock;
        }
        
        public void OnButtonClick()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = GetSpawnRotation();
            
            var command = new SpawnTowerCommand(towerFactory, towerType, spawnPosition, spawnRotation);
            CommandManager.Instance.ExecuteCommand(command);
            
            if (autoGrabOnSpawn)
            {
                StartCoroutine(AutoGrabSpawnedTower());
            }
        }
        
        private Vector3 GetSpawnPosition()
        {
            if (spawnAtPinchLocation && lastPinchPosition != Vector3.zero)
            {
                return lastPinchPosition;
            }
            
            return transform.position;
        }
        
        private Quaternion GetSpawnRotation()
        {
            if (Camera.main != null)
            {
                Vector3 lookDirection = Camera.main.transform.position - GetSpawnPosition();
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    return Quaternion.LookRotation(lookDirection);
                }
            }
            
            return Quaternion.identity;
        }
        
        private IEnumerator AutoGrabSpawnedTower()
        {
            yield return null;
            
            GameObject spawnedTower = FindRecentlySpawnedTower();
            
            if (spawnedTower != null)
            {
                XRGrabInteractable grabInteractable = 
                    spawnedTower.GetComponent<XRGrabInteractable>();
                
                if (grabInteractable != null)
                {
                    XRDirectInteractor handInteractor = GetHandInteractor();
                    
                    if (handInteractor != null)
                    {
                        handInteractor.StartManualInteraction((IXRSelectInteractable)grabInteractable);
                        
                        Debug.Log($"Auto-grabbed {towerType} tower at pinch location");
                    }
                    else
                    {
                        Debug.LogWarning("Could not find hand interactor for auto-grab");
                    }
                }
                else
                {
                    Debug.LogWarning("Spawned tower does not have XRGrabInteractable component");
                }
            }
            else
            {
                Debug.LogWarning("Could not find recently spawned tower for auto-grab");
            }
        }
        
        private GameObject FindRecentlySpawnedTower()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(GetSpawnPosition(), 0.1f);
            
            foreach (Collider col in nearbyObjects)
            {
                if (col.GetComponent<XRGrabInteractable>() != null)
                {
                    return col.gameObject;
                }
            }
            
            return null;
        }
        
        private XRDirectInteractor GetHandInteractor()
        {
            string handObjectName = useLeftHand ? "LeftHand" : "RightHand";
            
            GameObject handObject = GameObject.Find(handObjectName);
            if (handObject != null)
            {
                XRDirectInteractor interactor = 
                    handObject.GetComponentInChildren<XRDirectInteractor>();
                if (interactor != null)
                    return interactor;
            }
            
            XRDirectInteractor[] allInteractors = 
                FindObjectsOfType<XRDirectInteractor>();
            
            foreach (var interactor in allInteractors)
            {
                return interactor;
            }
            
            return null;
        }
        
        private void OnDrawGizmos()
        {
            Vector3 centerToShow = Application.isPlaying ? buttonCenter : GetPreviewCenter();
            
            if (Application.isPlaying)
            {
                Gizmos.color = isHovering ? Color.green : Color.red;
                Gizmos.DrawWireSphere(centerToShow, hoverDistance);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(centerToShow, 0.01f);
                
                if (isPinching)
                {
                    Gizmos.color = Color.orange;
                    Gizmos.DrawWireCube(centerToShow, Vector3.one * 0.02f);
                }
                
                if (lastHandDistance < float.MaxValue)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(centerToShow, 0.005f);
                }
                
                #if UNITY_EDITOR
                string stateInfo = isPinching ? "PINCHING" : isHovering ? "HOVERING" : "NORMAL";
                if (hasTriggeredPinchAction) stateInfo += " (TRIGGERED)";
                UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.05f, stateInfo);
                
                if (lastHandDistance < float.MaxValue)
                {
                    UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.1f, 
                        $"Hand: {lastHandDistance:F3}m");
                }
                
                if (lastPinchDistance < float.MaxValue)
                {
                    UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.15f, 
                        $"Pinch: {lastPinchDistance:F3}m");
                }
                #endif
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerToShow, hoverDistance);
                
                #if UNITY_EDITOR
                string configInfo = useManualCenter ? "Manual" : 
                                   centerTransform != null ? "Transform" : "Auto";
                UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.05f, 
                    $"{configInfo}: {centerToShow}");
                #endif
            }
        }
        
        private Vector3 GetPreviewCenter()
        {
            if (useManualCenter)
            {
                return manualButtonCenter;
            }
            
            if (centerTransform != null)
            {
                return centerTransform.position + centerOffset;
            }
            
            return transform.position + centerOffset;
        }
    }
}