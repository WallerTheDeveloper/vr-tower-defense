using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "WristMenuData", menuName = "UI Data/Wrist Menu Data", order = 1)]
    public class WristMenuData : ScriptableObject
    {
        public string itemHeader;
        public string itemDescription;
    }
}