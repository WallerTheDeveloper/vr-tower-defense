using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Pooling
{
    public class ObjectPoolManager : MonoBehaviour
    {
        [SerializeField] private bool addToDontDestroyOnLoad = false;

        private GameObject _emptyHolder;

        private static GameObject _particleSystemEmpty;
        private static GameObject _gameObjectsEmpty;
        
        private static Dictionary<GameObject, ObjectPool<GameObject>> _objectPool;
        private static Dictionary<GameObject, GameObject> _cloneToPrefabMap;

        public enum PoolType
        {
            ParticleSystem,
            GameObjects
        }
        
        public static ObjectPoolManager Instance;
        public static PoolType PoolingType;
        
        public static T SpawnObject<T>(T typePrefab, Vector3 spawnPos, Quaternion spawnRotation,
            PoolType poolType = PoolType.GameObjects) where T : Component
        {
            return SpawnObject<T>(typePrefab.gameObject, spawnPos, spawnRotation, poolType);
        }

        public static GameObject SpawnObject(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation,
            PoolType poolType = PoolType.GameObjects)
        {
            return SpawnObject<GameObject>(objectToSpawn, spawnPos, spawnRotation, poolType);
        }

        public static void ReturnObjectToPool(GameObject obj, PoolType poolType = PoolType.GameObjects)
        {
            if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
            {
                GameObject parentObject = SetParentObject(poolType);

                if (obj.transform.parent != parentObject.transform)
                {
                    obj.transform.SetParent(parentObject.transform);
                }

                if (_objectPool.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                {
                    pool.Release(obj);
                }
            }
            else
            {
                Debug.LogWarning($"Trying to return an object that is not pooled: {obj.name}");
            }
        }

        public static void ReturnObjectToPoolAfter(float time, GameObject obj, PoolType poolType = PoolType.GameObjects)
        {
            IEnumerator Execute()
            {
                yield return new WaitForSeconds(time);
        
                if (_cloneToPrefabMap.TryGetValue(obj, out GameObject prefab))
                {
                    GameObject parentObject = SetParentObject(poolType);

                    if (obj.transform.parent == parentObject.transform)
                    {
                        Debug.LogWarning($"Object {obj.name} has already been released to pool");
                        yield break;
                    }

                    obj.transform.SetParent(parentObject.transform);

                    if (_objectPool.TryGetValue(prefab, out ObjectPool<GameObject> pool))
                    {
                        pool.Release(obj);
                    }
                }
                else
                {
                    Debug.LogWarning($"Trying to return an object that is not pooled: {obj.name}");
                }
            }
            Instance.StartCoroutine(Execute());
        }
        
        private void Awake()
        {
            Instance = this;
            
            _objectPool  = new Dictionary<GameObject, ObjectPool<GameObject>>();
            _cloneToPrefabMap = new Dictionary<GameObject, GameObject>();

            SetupEmpties();
        }

        private void SetupEmpties()
        {
            _emptyHolder = new GameObject("Object Pools");
            
            _particleSystemEmpty = new GameObject("Particle Effects");
            _particleSystemEmpty.transform.SetParent(_emptyHolder.transform);
            
            _gameObjectsEmpty = new GameObject("GameObjects");
            _gameObjectsEmpty.transform.SetParent(_emptyHolder.transform);

            if (addToDontDestroyOnLoad)
            {
                DontDestroyOnLoad(_particleSystemEmpty.transform.root);
            }
        }

        private static void CreatePool(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObjects)
        {
            ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
                createFunc: () => CreateObject(prefab, position, rotation, poolType),
                actionOnGet: OnGetObject,
                actionOnRelease: OnReleaseObject,
                actionOnDestroy: OnDestroyObject);
            
            _objectPool.Add(prefab, pool);
        }

        private static GameObject CreateObject(GameObject prefab, Vector3 position, Quaternion rotation,
            PoolType poolType = PoolType.GameObjects)
        {
            prefab.SetActive(false);
            
            GameObject obj = Instantiate(prefab, position, rotation);
            prefab.SetActive(true);
            
            GameObject parentObject = SetParentObject(poolType);
            obj.transform.SetParent(parentObject.transform);

            return obj;
        }

        private static void OnGetObject(GameObject obj)
        {
            
        }

        private static void OnReleaseObject(GameObject obj)
        {
            obj.SetActive(false);
        }

        private static void OnDestroyObject(GameObject obj)
        {
            if (_cloneToPrefabMap.ContainsKey(obj))
            {
                _cloneToPrefabMap.Remove(obj);
            }
        }
        
        private static GameObject SetParentObject(PoolType poolType)
        {
            switch (poolType)
            {
                case PoolType.ParticleSystem:
                {
                    return _particleSystemEmpty;
                }
                case PoolType.GameObjects:
                {
                    return _gameObjectsEmpty;
                }
                default:
                {
                    return null;
                }
            }
        }

        private static T SpawnObject<T>(GameObject objectToSpawn, Vector3 spawnPos, Quaternion spawnRotation,
            PoolType poolType = PoolType.GameObjects) where T : Object
        {
            if (!_objectPool.ContainsKey(objectToSpawn))
            {
                CreatePool(objectToSpawn, spawnPos, spawnRotation, poolType);
            }
            
            GameObject obj = _objectPool[objectToSpawn].Get();

            if (obj != null)
            {
                if (!_cloneToPrefabMap.ContainsKey(obj))
                {
                    _cloneToPrefabMap.Add(obj, objectToSpawn);
                }
                
                obj.transform.position = spawnPos;
                obj.transform.rotation = spawnRotation;
                obj.SetActive(true);

                if (typeof(T) == typeof(GameObject))
                {
                    return obj as T;
                }
                
                T component = obj.GetComponent<T>();
                if (component == null)
                {
                    Debug.LogError($"$Object {objectToSpawn.name} has no component of type {typeof(T)}");
                }
                
                return component;
            }

            return null;
        }
    }
}