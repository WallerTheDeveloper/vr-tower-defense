using System.Collections;
using Core.Commands;
using Core.Factories;
using Hands;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.XR.Hands;

public class TowerMenuButton : MonoBehaviour
    {
        [Header("Tower Settings")]
        [SerializeField] private TowerType towerType;
        [SerializeField] private TowerFactory towerFactory;
        
        [Header("Hover Detection")]
        [SerializeField] private float hoverDistance = 0.05f; // 5cm
        [SerializeField] private float hoverTime = 0.3f; // Time to hold hover for selection
        [SerializeField] private bool useLeftHand = true;
        
        [Header("Position Configuration")]
        [SerializeField] private bool useManualCenter = false;
        [SerializeField] private Vector3 manualButtonCenter = Vector3.zero;
        [SerializeField] private Transform centerTransform; // Alternative: use another transform as center
        [SerializeField] private Vector3 centerOffset = Vector3.zero; // Offset from calculated center
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip selectSound;
        
        [Header("Events")]
        public UnityEvent OnHoverEnter;
        public UnityEvent OnHoverExit;
        public UnityEvent OnHoverSelect;
        
        private bool isHovering = false;
        private bool wasHovering = false;
        private float hoverStartTime = 0f;
        private Vector3 buttonCenter;
        private Button button;
        private AudioSource audioSource;
        private XRHandSubsystem handSubsystem;
        private float lastHandDistance = float.MaxValue;
        
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
            HandleHoverLogic();
        }
        
        private void UpdateButtonCenter()
        {
            // Manual center override
            if (useManualCenter)
            {
                buttonCenter = manualButtonCenter;
                return;
            }
            
            // Use another transform as center
            if (centerTransform != null)
            {
                buttonCenter = centerTransform.position + centerOffset;
                return;
            }
            
            // Automatic calculation with offset
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
                
                // Check if hand is within hover distance
                bool withinDistance = distance <= hoverDistance;
                
                if (withinDistance)
                {
                    // Request hover from the manager (only closest button gets it)
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
        
        private void HandleHoverLogic()
        {
            // Hover Enter
            if (isHovering && !wasHovering)
            {
                OnHoverEnterAction();
                hoverStartTime = Time.time;
            }
            // Hover Exit
            else if (!isHovering && wasHovering)
            {
                OnHoverExitAction();
                hoverStartTime = 0f;
            }
            // Hover Hold (for selection)
            else if (isHovering && wasHovering)
            {
                float hoverDuration = Time.time - hoverStartTime;
                if (hoverDuration >= hoverTime)
                {
                    OnHoverSelectAction();
                    hoverStartTime = Time.time; // Reset but keep hovering state
                }
            }
            
            wasHovering = isHovering;
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
                hoverStartTime = 0f;
            }
        }
        
        private void OnHoverEnterAction()
        {
            UpdateVisualState(ButtonState.Hover);
            
            if (hoverSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hoverSound);
            }
            
            OnHoverEnter?.Invoke();
            
            Debug.Log($"Hovering over {towerType} tower button at position {buttonCenter}");
        }
        
        private void OnHoverExitAction()
        {
            UpdateVisualState(ButtonState.Normal);
            
            OnHoverExit?.Invoke();
            
            Debug.Log($"Stopped hovering over {towerType} tower button");
        }
        
        private void OnHoverSelectAction()
        {
            UpdateVisualState(ButtonState.Selected);
            
            if (selectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(selectSound);
            }
            
            OnHoverSelect?.Invoke();
            
            // Execute tower spawn command
            OnButtonClick();
            
            Debug.Log($"Selected {towerType} tower button via hover");
            
            // Reset visual state after a short delay
            StartCoroutine(ResetVisualStateAfterDelay(0.2f));
        }
        
        private IEnumerator ResetVisualStateAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            UpdateVisualState(ButtonState.Normal);
        }
        
        private void UpdateVisualState(ButtonState state)
        {
            if (button == null) return;
            
            Color targetColor = state switch
            {
                ButtonState.Normal => normalColor,
                ButtonState.Hover => hoverColor,
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
            var command = new SpawnTowerCommand(towerFactory, towerType, transform.position, Quaternion.identity);
            CommandManager.Instance.ExecuteCommand(command);
        }
        
        // Gizmos for debugging
        private void OnDrawGizmos()
        {
            Vector3 centerToShow = Application.isPlaying ? buttonCenter : GetPreviewCenter();
            
            if (Application.isPlaying)
            {
                // Draw hover detection sphere
                Gizmos.color = isHovering ? Color.green : Color.red;
                Gizmos.DrawWireSphere(centerToShow, hoverDistance);
                
                // Draw button center
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(centerToShow, 0.01f);
                
                // Show distance from hand
                if (lastHandDistance < float.MaxValue)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawWireSphere(centerToShow, 0.005f);
                }
                
                #if UNITY_EDITOR
                if (isHovering)
                {
                    float hoverDuration = Time.time - hoverStartTime;
                    UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.05f, 
                        $"Hover: {hoverDuration:F2}s / {hoverTime:F2}s");
                }
                
                // Always show distance info
                if (lastHandDistance < float.MaxValue)
                {
                    UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.1f, 
                        $"Dist: {lastHandDistance:F3}m");
                }
                #endif
            }
            else
            {
                // Show hover distance in editor
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(centerToShow, hoverDistance);
                
                // Show configuration info in editor
                #if UNITY_EDITOR
                string configInfo = useManualCenter ? "Manual" : 
                                   centerTransform != null ? "Transform" : "Auto";
                UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.05f, 
                    $"{configInfo}: {centerToShow}");
                #endif
            }
        }
        
        // Helper method to preview center position in editor
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
            
            // Auto calculation preview
            return transform.position + centerOffset;
        }
    }