using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [SerializeField] private List<GameObject> objectPrefabs; // Assign all object prefabs here (cars, fruits, etc.)
    [SerializeField] private int defaultPoolSize = 200;         // Number of instances per object type.

    // Dictionary: key = prefab name, value = queue of pooled object instances.
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally: DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // For each object prefab, create its pool.
        foreach (GameObject prefab in objectPrefabs)
        {
            string tag = prefab.name;
            Queue<GameObject> pool = new Queue<GameObject>();
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
    public GameObject GetObjectFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Object pool with tag '{tag}' does not exist.");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        GameObject obj;

        if (pool.Count == 0)
        {
            Debug.LogWarning($"Object pool for tag '{tag}' is empty. Instantiating a new object.");
            GameObject prefab = objectPrefabs.Find(x => x.name == tag);
            if (prefab == null)
            {
                Debug.LogWarning($"No object prefab found for tag '{tag}'.");
                return null;
            }
            obj = Instantiate(prefab, position, rotation);
        }
        else
        {
            obj = pool.Dequeue();
            obj.transform.position = position;
            obj.transform.rotation = rotation;
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
