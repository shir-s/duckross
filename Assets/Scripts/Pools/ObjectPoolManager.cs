using System.Collections.Generic;
using UnityEngine;

namespace Pools
{
    public class ObjectPoolManager : MonoBehaviour
    {
        public static ObjectPoolManager Instance { get; private set; }

        [SerializeField] private List<GameObject> objectPrefabs; // Assign all object prefabs here.
        [SerializeField] private int defaultPoolSize = 200;         // Number of instances per object type.

        // Dictionary that maps a prefab name to its pool.
        private Dictionary<string, Queue<GameObject>> poolDictionary;
        // Dictionary to store each prefab's original rotation.
        private Dictionary<string, Quaternion> prefabRotations;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
            prefabRotations = new Dictionary<string, Quaternion>();

            // For each object prefab, create its pool and store its original rotation.
            foreach (GameObject prefab in objectPrefabs)
            {
                string tag = prefab.name;
                Queue<GameObject> pool = new Queue<GameObject>();

                // Store the prefab's original rotation.
                if (!prefabRotations.ContainsKey(tag))
                {
                    prefabRotations.Add(tag, prefab.transform.rotation);
                }

                for (int i = 0; i < defaultPoolSize; i++)
                {
                    GameObject obj = Instantiate(prefab);
                    obj.SetActive(false);
                    pool.Enqueue(obj);
                }

                poolDictionary.Add(tag, pool);
            }
        }

        // Retrieves an object from the pool (or instantiates a new one if empty).
        public GameObject GetObjectFromPool(string tag, Vector3 position)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"Object pool with tag '{tag}' does not exist.");
                return null;
            }

            Queue<GameObject> pool = poolDictionary[tag];
            GameObject obj;
    
            // Look up the original rotation from our stored prefab rotations.
            Quaternion originalRotation = prefabRotations.ContainsKey(tag) ? prefabRotations[tag] : Quaternion.identity;

            if (pool.Count == 0)
            {
                Debug.LogWarning($"Object pool for tag '{tag}' is empty. Instantiating a new object.");
                GameObject prefab = objectPrefabs.Find(x => x.name == tag);
                if (prefab == null)
                {
                    Debug.LogWarning($"No object prefab found for tag '{tag}'.");
                    return null;
                }
                obj = Instantiate(prefab, position, originalRotation);
            }
            else
            {
                obj = pool.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = originalRotation;
                obj.SetActive(true);
            }

            return obj;
        }

        // Returns an object to its pool.
        public void ReturnObjectToPool(string tag, GameObject obj)
        {
            obj.SetActive(false);
            if (poolDictionary.ContainsKey(tag))
            {
                poolDictionary[tag].Enqueue(obj);
            }
            else
            {
                Debug.LogWarning($"Attempted to return an object for tag '{tag}', but no pool exists.");
            }
        }
    }
}

