using UnityEngine;

namespace Core
{
    public class GameController : MonoBehaviour
    {
        private static GameController _instance;
        public static GameController Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindAnyObjectByType<GameController>();
                    
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("GameController");
                        _instance = managerObject.AddComponent<GameController>();
                        DontDestroyOnLoad(managerObject);
                        Debug.Log("[GameController] Created new instance");
                    }
                }
                return _instance;
            }
        }

        public static bool IsGameOver { get; set; }
    }
}