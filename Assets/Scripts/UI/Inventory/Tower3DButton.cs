using System.Collections;
using Core.Commands;
using Core.Commands.ConcreteCommands;
using Core.Factories;
using Data.UI;
using Hands;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace UI.Inventory
{
    public class Tower3DButton : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private WristMenuData data;
        [FormerlySerializedAs("towerFactory")]
        [Header("Tower Settings")]
        [SerializeField] private UnitFactory unitFactory;
        
        [Header("3D Model")]
        [SerializeField] private GameObject tower3DModel;
        [SerializeField] private Transform modelContainer;
        [SerializeField] private Vector3 modelScale = Vector3.one;
        [SerializeField] private Vector3 modelRotationOffset = Vector3.zero;
        
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
        
        [Header("Visual Feedback")]
        [SerializeField] private Color normalEmissionColor = Color.black;
        [SerializeField] private Color hoverEmissionColor = Color.yellow;
        [SerializeField] private Color pinchingEmissionColor = Color.orange;
        [SerializeField] private Color selectedEmissionColor = Color.green;
        [SerializeField] private float emissionIntensity = 2f;
        [SerializeField] private float scaleMultiplierOnHover = 1.1f;
        [SerializeField] private float scaleMultiplierOnPinch = 1.2f;
        [SerializeField] private float animationSpeed = 5f;
        
        [Header("Audio Feedback")]
        [SerializeField] private AudioClip hoverSound;
        [SerializeField] private AudioClip pinchStartSound;
        [SerializeField] private AudioClip pinchReleaseSound;
        
        [Header("UI References")] 
        [SerializeField] private TextMeshProUGUI itemHeader;
        [SerializeField] private TextMeshProUGUI itemDescription;
        [SerializeField] private CanvasGroup menuParentMenuCanvasGroup;
        
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
        private AudioSource audioSource;
        private XRHandSubsystem handSubsystem;
        private float lastHandDistance = float.MaxValue;
        private float lastPinchDistance = float.MaxValue;
        
        private GameObject instantiated3DModel;
        private Renderer[] modelRenderers;
        private Material[] originalMaterials;
        private Material[] emissionMaterials;
        private Vector3 originalScale;
        private Vector3 targetScale;
        
        private HandHoverDetector handHoverDetector;
        
        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            
            InitializeHandTracking();
            Initialize3DModel();
            
            handHoverDetector = FindFirstObjectByType<HandHoverDetector>();
            
            UpdateVisualState(ButtonState.Normal);
        }
        
        private void Initialize3DModel()
        {
            if (tower3DModel == null)
            {
                Debug.LogError($"Tower 3D Model is not assigned for {gameObject.name}");
                return;
            }
            
            if (modelContainer == null)
            {
                GameObject container = new GameObject("ModelContainer");
                container.transform.SetParent(transform);
                container.transform.localPosition = Vector3.zero;
                container.transform.localRotation = Quaternion.identity;
                modelContainer = container.transform;
            }
            
            instantiated3DModel = Instantiate(tower3DModel, modelContainer);
            instantiated3DModel.transform.localPosition = Vector3.zero;
            instantiated3DModel.transform.localRotation = Quaternion.Euler(modelRotationOffset);
            instantiated3DModel.transform.localScale = modelScale;
            
            originalScale = instantiated3DModel.transform.localScale;
            targetScale = originalScale;
            
            RemoveInteractionComponents(instantiated3DModel);
            
            modelRenderers = instantiated3DModel.GetComponentsInChildren<Renderer>();
            SetupEmissionMaterials();
        }
        
        private void RemoveInteractionComponents(GameObject obj)
        {
            // Remove grab interactables and colliders to prevent interference
            XRGrabInteractable[] grabInteractables = obj.GetComponentsInChildren<XRGrabInteractable>();
            foreach (var grab in grabInteractables)
            {
                DestroyImmediate(grab);
            }
            
            Collider[] colliders = obj.GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.isTrigger = true;
            }
        }
        
        private void SetupEmissionMaterials()
        {
            if (modelRenderers == null) return;
            
            originalMaterials = new Material[modelRenderers.Length];
            emissionMaterials = new Material[modelRenderers.Length];
            
            for (int i = 0; i < modelRenderers.Length; i++)
            {
                if (modelRenderers[i].material != null)
                {
                    originalMaterials[i] = modelRenderers[i].material;
                    
                    emissionMaterials[i] = new Material(originalMaterials[i]);
                    
                    emissionMaterials[i].EnableKeyword("_EMISSION");
                    
                    emissionMaterials[i].globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    
                    emissionMaterials[i].SetFloat("_UseEmission", 1f);
                    
                    emissionMaterials[i].SetColor("_EmissionColor", Color.black);
                    
                    modelRenderers[i].material = emissionMaterials[i];
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
            }
        }
        
        private void Update()
        {
            UpdateButtonCenter();
            CheckHandHover();
            CheckPinchGesture();
            HandleInteractionLogic();
            UpdateModelAnimation();
        }
        
        private void UpdateButtonCenter()
        {
            if (modelContainer != null)
            {
                buttonCenter = modelContainer.position;
            }
            else if (instantiated3DModel != null)
            {
                buttonCenter = instantiated3DModel.transform.position;
            }
            else
            {
                buttonCenter = transform.position;
            }
        }
        
        private void UpdateModelAnimation()
        {
            if (instantiated3DModel == null) return;
            
            UpdateModelTransparency();
            
            instantiated3DModel.transform.localScale = Vector3.Lerp(
                instantiated3DModel.transform.localScale, 
                targetScale, 
                Time.deltaTime * animationSpeed
            );
            
            if (isHovering)
            {
                float floatOffset = Mathf.Sin(Time.time * 3f) * 0.005f;
                Vector3 currentPos = instantiated3DModel.transform.localPosition;
                currentPos.y = floatOffset;
                instantiated3DModel.transform.localPosition = currentPos;
            }
            else
            {
                Vector3 currentPos = instantiated3DModel.transform.localPosition;
                currentPos.y = Mathf.Lerp(currentPos.y, 0f, Time.deltaTime * animationSpeed);
                instantiated3DModel.transform.localPosition = currentPos;
            }
        }
        
        private void UpdateModelTransparency()
        {
            menuParentMenuCanvasGroup = GetComponentInParent<CanvasGroup>();
            if (menuParentMenuCanvasGroup == null) return;
            
            float targetAlpha = menuParentMenuCanvasGroup.alpha;
            
            foreach (var material in emissionMaterials)
            {
                if (material != null)
                {
                    SetMaterialTransparency(material, targetAlpha);
                }
            }
        }
        
        private void SetMaterialTransparency(Material material, float alpha)
        {
            if (material.HasProperty("_BaseColor"))
            {
                Color baseColor = material.GetColor("_BaseColor");
                baseColor.a = alpha;
                material.SetColor("_BaseColor", baseColor);
            }
            
            if (alpha < 1f)
            {
                material.SetFloat("_Surface", 1);
                material.SetFloat("_Blend", 0);
                
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                material.SetFloat("_Surface", 0);
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
            }
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
                    SetHeaderAndDescriptionData(data.itemHeader, data.itemDescription);
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
        
        private void SetHeaderAndDescriptionData(string header, string description)
        {
            if (itemHeader != null) itemHeader.text = header;
            if (itemDescription != null) itemDescription.text = description;
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
            if (emissionMaterials == null) return;
            
            Color targetEmissionColor;
            float targetScaleMultiplier = 1f;
            
            switch (state)
            {
                case ButtonState.Normal:
                    targetEmissionColor = normalEmissionColor;
                    targetScaleMultiplier = 1f;
                    break;
                case ButtonState.Hover:
                    targetEmissionColor = hoverEmissionColor;
                    targetScaleMultiplier = scaleMultiplierOnHover;
                    break;
                case ButtonState.Pinching:
                    targetEmissionColor = pinchingEmissionColor;
                    targetScaleMultiplier = scaleMultiplierOnPinch;
                    break;
                case ButtonState.Selected:
                    targetEmissionColor = selectedEmissionColor;
                    targetScaleMultiplier = scaleMultiplierOnPinch;
                    break;
                default:
                    targetEmissionColor = normalEmissionColor;
                    targetScaleMultiplier = 1f;
                    break;
            }
            
            foreach (var material in emissionMaterials)
            {
                if (material != null)
                {
                    material.SetColor("_EmissionColor", targetEmissionColor * emissionIntensity);
                }
            }
            
            targetScale = originalScale * targetScaleMultiplier;
        }
        
        public void OnButtonClick()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            Quaternion spawnRotation = GetSpawnRotation();
            
            var command = new SpawnTowerCommand(unitFactory, spawnPosition, spawnRotation);
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
            yield return new WaitForSeconds(0.1f);
            
            Vector3 spawnPos = GetSpawnPosition();
            Debug.Log($"Attempting to auto-grab tower at position: {spawnPos}");
            
            GameObject spawnedTower = FindRecentlySpawnedTower();
            
            if (spawnedTower != null)
            {
                Debug.Log($"Found tower: {spawnedTower.name} at position: {spawnedTower.transform.position}");
                
                XRGrabInteractable grabInteractable = spawnedTower.GetComponent<XRGrabInteractable>();
                
                if (grabInteractable != null)
                {
                    if (!grabInteractable.enabled)
                    {
                        Debug.LogWarning("XRGrabInteractable is disabled, enabling it...");
                        grabInteractable.enabled = true;
                    }
                    
                    XRDirectInteractor handInteractor = GetHandInteractor();
                    
                    if (handInteractor != null)
                    {
                        Debug.Log($"Attempting to grab with interactor: {handInteractor.name}");
                        
                        bool grabSuccessful = false;
                        
                        try
                        {
                            handInteractor.StartManualInteraction((IXRSelectInteractable)grabInteractable);
                            grabSuccessful = true;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogWarning($"Manual interaction failed: {e.Message}");
                        }
                        
                        if (!grabSuccessful)
                        {
                            yield return new WaitForSeconds(0.1f);
                            
                            try
                            {
                                if (handInteractor.CanSelect((IXRSelectInteractable)grabInteractable))
                                {
                                    handInteractor.StartManualInteraction((IXRSelectInteractable)grabInteractable);
                                    grabSuccessful = true;
                                }
                                else
                                {
                                    Debug.LogWarning("Interactor cannot select the grab interactable");
                                }
                            }
                            catch (System.Exception e)
                            {
                                Debug.LogWarning($"Force selection failed: {e.Message}");
                            }
                        }
                        
                        if (!grabSuccessful)
                        {
                            Debug.LogError("All auto-grab methods failed");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Could not find hand interactor for auto-grab");
                    }
                }
                else
                {
                    Debug.LogWarning($"Spawned tower {spawnedTower.name} does not have XRGrabInteractable component");
                }
            }
            else
            {
                Debug.LogWarning("Could not find recently spawned tower for auto-grab");
            }
        }
        
        private GameObject FindRecentlySpawnedTower()
        {
            Vector3 spawnPosition = GetSpawnPosition();
            
            float[] searchRadii = { 0.1f, 0.2f, 0.5f, 1.0f };
            
            foreach (float radius in searchRadii)
            {
                Collider[] nearbyObjects = Physics.OverlapSphere(spawnPosition, radius);
                
                foreach (Collider col in nearbyObjects)
                {
                    if (col.GetComponent<XRGrabInteractable>() != null)
                    {
                        GameObject tower = col.gameObject;
                        
                        Debug.Log($"Found tower at distance {Vector3.Distance(spawnPosition, tower.transform.position):F3}m");
                        return tower;
                    }
                }
            }
            
            XRGrabInteractable[] allGrabInteractables = FindObjectsOfType<XRGrabInteractable>();
            
            GameObject closestTower = null;
            float closestDistance = float.MaxValue;
            
            foreach (var grabInteractable in allGrabInteractables)
            {
                float distance = Vector3.Distance(spawnPosition, grabInteractable.transform.position);
                
                if (distance < 2.0f && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTower = grabInteractable.gameObject;
                }
            }
            
            if (closestTower != null)
            {
                Debug.Log($"Found closest tower at distance {closestDistance:F3}m using fallback method");
                return closestTower;
            }
            
            Debug.LogWarning($"Could not find any towers near spawn position {spawnPosition}. Checked {allGrabInteractables.Length} total grab interactables.");
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
                UnityEditor.Handles.Label(centerToShow + Vector3.up * 0.05f, 
                    $"3D Button: {centerToShow}");
                #endif
            }
        }
        
        private Vector3 GetPreviewCenter()
        {
            if (modelContainer != null)
            {
                return modelContainer.position;
            }
            
            return transform.position;
        }
        
        private void OnDestroy()
        {
            if (emissionMaterials != null)
            {
                foreach (var material in emissionMaterials)
                {
                    if (material != null)
                    {
                        DestroyImmediate(material);
                    }
                }
            }
        }
    }
}