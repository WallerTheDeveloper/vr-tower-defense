using System.Collections;
using UnityEngine;

namespace Core
{
    public class ParticleEffectManager : MonoBehaviour
    {
        private static ParticleEffectManager _instance;
        
        public static ParticleEffectManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Try to find existing instance in scene
                    _instance = FindAnyObjectByType<ParticleEffectManager>();
                    
                    // Create new instance if none exists
                    if (_instance == null)
                    {
                        GameObject managerObject = new GameObject("ParticleEffectManager");
                        _instance = managerObject.AddComponent<ParticleEffectManager>();
                        DontDestroyOnLoad(managerObject);
                        Debug.Log("[ParticleEffectManager] Created new instance");
                    }
                }
                return _instance;
            }
        }
        
        public void DestroyParticleEffectAfter(float time, ParticleSystem particleEffect)
        {
            IEnumerator Execute(float effectTotalTime, ParticleSystem particleEffect)
            {
                while (particleEffect.totalTime < effectTotalTime)
                {
                    yield return null;
                }
                Destroy(particleEffect.gameObject);   
            }
            StartCoroutine(Execute(time, particleEffect));
        }
    }
}