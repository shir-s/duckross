using System.Collections.Generic;
using UnityEngine;

public class SpawnerPoolManager : MonoBehaviour
{
    public static SpawnerPoolManager Instance { get; private set; }

    [SerializeField] private List<GameObject> spawnerPrefabs; // Assign all spawner prefabs here.
    [SerializeField] private int defaultPoolSize = 10; // Number of instances per spawner type.

    // Dictionary: key = spawner prefab name, value = pool (queue) of spawner instances.
    private Dictionary<string, Queue<GameObject>> spawnerPoolDictionary;

    private void Awake()
    {
        // Singleton pattern: if an instance already exists, destroy this one.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally, make this persistent across scenes:
        // DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        spawnerPoolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (GameObject prefab in spawnerPrefabs)
        {
            string tag = prefab.name;
            Queue<GameObject> pool = new Queue<GameObject>();
            for (int i = 0; i < defaultPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
            spawnerPoolDictionary.Add(tag, pool);
        }
    }

    // Retrieves a spawner by tag, sets its position/rotation, and returns it.
    // If the pool is empty, a new spawner is instantiated automatically.
    public GameObject GetSpawnerFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!spawnerPoolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Spawner pool with tag '{tag}' does not exist.");
            return null;
        }

        Queue<GameObject> pool = spawnerPoolDictionary[tag];
        GameObject spawnerObj;

        if (pool.Count == 0)
        {
            Debug.LogWarning($"Spawner pool for tag '{tag}' is empty. Instantiating a new spawner.");
            // Find the corresponding prefab by tag.
            GameObject prefab = spawnerPrefabs.Find(p => p.name == tag);
            if (prefab == null)
            {
                Debug.LogWarning($"No spawner prefab found for tag '{tag}'.");
                return null;
            }
            spawnerObj = Instantiate(prefab, position, rotation);
        }
        else
        {
            spawnerObj = pool.Dequeue();
            spawnerObj.SetActive(true);
            spawnerObj.transform.position = position;
            spawnerObj.transform.rotation = rotation;
        }

        return spawnerObj;
    }

    // Returns a spawner to its pool.
    public void ReturnSpawnerToPool(string tag, GameObject spawnerObj)
    {
        spawnerObj.SetActive(false);
        if (spawnerPoolDictionary.ContainsKey(tag))
        {
            spawnerPoolDictionary[tag].Enqueue(spawnerObj);
        }
        else
        {
            Debug.LogWarning($"Attempted to return a spawner for tag '{tag}', but no pool exists.");
        }
    }
}
