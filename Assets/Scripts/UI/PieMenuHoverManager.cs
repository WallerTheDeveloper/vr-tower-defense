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