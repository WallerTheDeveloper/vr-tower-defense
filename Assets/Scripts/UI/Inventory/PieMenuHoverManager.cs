using UnityEngine;

namespace UI.Inventory
{
    public class PieMenuHoverManager : MonoBehaviour
    {
        private static PieMenuHoverManager _instance;

        public static PieMenuHoverManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("PieMenuHoverManager");
                    _instance = go.AddComponent<PieMenuHoverManager>();
                    DontDestroyOnLoad(go);
                }

                return _instance;
            }
        }

        private Tower3DButton currentHoveredButton;
        private Tower3DButton currentPinchingButton;
        private Tower3DButton buttonThatTriggeredPinchAction;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
        }

        public bool RequestHover(Tower3DButton button, float distance)
        {
            if (currentHoveredButton == null)
            {
                SetHoveredButton(button);
                return true;
            }

            if (currentHoveredButton == button)
            {
                return true;
            }

            float currentDistance = currentHoveredButton.GetHandDistance();
            if (distance < currentDistance)
            {
                currentHoveredButton.ForceHoverExit();
                SetHoveredButton(button);
                return true;
            }

            return false;
        }

        public void ReleaseHover(Tower3DButton button)
        {
            if (currentHoveredButton == button)
            {
                currentHoveredButton = null;
            }
        }

        public bool RequestPinch(Tower3DButton button, float distance)
        {
            if (currentPinchingButton == null)
            {
                SetPinchingButton(button);
                return true;
            }

            if (currentPinchingButton == button)
            {
                return true;
            }

            float currentDistance = currentPinchingButton.GetHandDistance();
            if (distance < currentDistance)
            {
                currentPinchingButton.ForcePinchExit();
                SetPinchingButton(button);
                return true;
            }

            return false;
        }

        public void ReleasePinch(Tower3DButton button)
        {
            if (currentPinchingButton == button)
            {
                currentPinchingButton = null;
            }
        }

        public bool HasAnyButtonTriggeredPinchAction()
        {
            return buttonThatTriggeredPinchAction != null;
        }

        public void SetPinchActionTriggered(Tower3DButton button)
        {
            if (buttonThatTriggeredPinchAction == null)
            {
                buttonThatTriggeredPinchAction = button;
            }
        }

        public void ClearPinchActionTrigger()
        {
            if (buttonThatTriggeredPinchAction != null)
            {
                Debug.Log($"Clearing pinch action trigger from button: {buttonThatTriggeredPinchAction.name}");
                buttonThatTriggeredPinchAction = null;
            }
        }

        public Tower3DButton GetButtonThatTriggeredPinchAction()
        {
            return buttonThatTriggeredPinchAction;
        }

        private void SetHoveredButton(Tower3DButton button)
        {
            currentHoveredButton = button;
        }

        private void SetPinchingButton(Tower3DButton button)
        {
            currentPinchingButton = button;
        }

        private void OnGUI()
        {
            if (Application.isPlaying && Debug.isDebugBuild)
            {
                GUILayout.BeginArea(new Rect(10, 10, 300, 100));
                GUILayout.Label($"Hovered: {(currentHoveredButton != null ? currentHoveredButton.name : "None")}");
                GUILayout.Label($"Pinching: {(currentPinchingButton != null ? currentPinchingButton.name : "None")}");
                GUILayout.Label(
                    $"Action Triggered: {(buttonThatTriggeredPinchAction != null ? buttonThatTriggeredPinchAction.name : "None")}");
                GUILayout.EndArea();
            }
        }
    }
}