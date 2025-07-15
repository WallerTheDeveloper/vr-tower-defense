using UnityEngine;

namespace UI
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
        
        private TowerMenuButton currentHoveredButton;
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }
        
        public bool RequestHover(TowerMenuButton button, float distance)
        {
            // If no button is currently hovered, or this button is closer than the current one
            if (currentHoveredButton == null)
            {
                SetHoveredButton(button);
                return true;
            }
            
            // If the same button is requesting hover, keep it
            if (currentHoveredButton == button)
            {
                return true;
            }
            
            // Check if this button is closer than the currently hovered one
            float currentDistance = currentHoveredButton.GetHandDistance();
            if (distance < currentDistance)
            {
                // Exit the current button and set the new one
                currentHoveredButton.ForceHoverExit();
                SetHoveredButton(button);
                return true;
            }
            
            return false;
        }
        
        public void ReleaseHover(TowerMenuButton button)
        {
            if (currentHoveredButton == button)
            {
                currentHoveredButton = null;
            }
        }
        
        private void SetHoveredButton(TowerMenuButton button)
        {
            currentHoveredButton = button;
        }
    }
}