using System.Collections.Generic;
using UnityEngine;

public class SegmentPoolManager : MonoBehaviour
{
    // List of segment prefabs to pool (set these via the Inspector).
    [SerializeField] private List<GameObject> segmentPrefabs;

    // How many instances to pre-instantiate for each segment type.
    [SerializeField] private int defaultPoolSize = 100;

    // Dictionary that maps a segment tag (using the prefab's name) to its pool (queue of GameObjects).
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        // Iterate over the prefabs provided in the serialized list.
        foreach (GameObject prefab in segmentPrefabs)
        {
            string tag = prefab.name;
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < defaultPoolSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(tag, objectPool);
        }
    }

    // Pulls a segment from the pool by its tag, positions it, and returns it.
    // If the pool is empty, it instantiates a new segment.
    public GameObject GetSegmentFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"Pool with tag '{tag}' doesn't exist.");
            return null;
        }

        Queue<GameObject> pool = poolDictionary[tag];
        GameObject segment;

        // If the pool is empty, instantiate a new segment.
        if (pool.Count == 0)
        {
            Debug.LogWarning($"Segment pool for tag '{tag}' is empty. Instantiating a new segment.");
            GameObject prefab = segmentPrefabs.Find(x => x.name == tag);
            if (prefab == null)
            {
                Debug.LogWarning($"No segment prefab found for tag '{tag}'.");
                return null;
            }
            segment = Instantiate(prefab, position, rotation);
        }
        else
        {
            segment = pool.Dequeue();
            segment.transform.position = position;
            segment.transform.rotation = rotation;
            segment.SetActive(true);
        }

        return segment;
    }

    // Returns a segment to its pool.
    public void ReturnSegmentToPool(string tag, GameObject segment)
    {
        segment.SetActive(false);
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(segment);
        }
        else
        {
            Debug.LogWarning($"Attempted to return a segment for tag '{tag}', but no pool exists.");
        }
    }
}
