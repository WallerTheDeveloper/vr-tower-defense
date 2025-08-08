using UnityEngine;

namespace Data.UI
{
    [CreateAssetMenu(fileName = "Wrist Menu Data", menuName = "Data/Wrist Menu Data")]
    public class WristMenuData : ScriptableObject
    {
        public string itemHeader;
        public string itemDescription;
    }
}