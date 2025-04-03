using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class InfiniteWorldGenerator : MonoBehaviour, IGameStateListener
{
    [SerializeField] private SegmentPoolManager poolManager;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 20f;   // When to spawn a new segment (based on player's z)
    [SerializeField] private float destroyDistanceBehind = 15f;  // When to remove an old segment (based on player's z)
    [SerializeField] private int initialSegmentCount = 5;        // Additional segments to spawn after initial safe zones.

    // List of base segment kinds (e.g., "RoadSegment", "FruitSegment", "SafeZone", etc.).
    // For each type, you must have corresponding prefabs with names that follow:
    //   Base, BaseStart, and BaseMid (and BaseEnd if needed).
    [SerializeField] private List<string> segmentTags;

    // World start position on the z axis.
    [SerializeField] private float worldStartZ = 0f;

    // Group spawn settings – each group (except safe zones) will contain between these many segments.
    [SerializeField] private int groupMinCount = 3;
    [SerializeField] private int groupMaxCount = 7;

    // New: Settings for safe zones.
    [SerializeField] private int initialSafeZoneCount = 6; // Always spawn exactly these many at game start.
    [SerializeField] private string safeZoneTag = "SafeSegment"; // The base tag for safe zone segments.

    // Instead of using player position as our base, we keep track of where the next segment should be placed.
    private float nextSegmentZ;
    
    // Queue to keep track of active segments.
    private Queue<GameObject> activeSegments = new Queue<GameObject>();

    // Counters to track segment types.
    private string previousSegmentTag = "";
    private bool firstSegment;

    // Flag to control generator activity.
    private bool isGameActive = false;

    void Update()
    {
        if (!isGameActive)
            return;

        // Spawn a new group of segments when the player's z plus spawnDistanceAhead exceeds nextSegmentZ.
        if (player.position.z + spawnDistanceAhead > nextSegmentZ)
        {
            SpawnSegment();
        }

        // Remove segments that have fallen too far behind the player.
        if (activeSegments.Count > 0)
        {
            GameObject oldestSegment = activeSegments.Peek();
            if (player.position.z - oldestSegment.transform.position.z > destroyDistanceBehind)
            {
                string tag = oldestSegment.name.Replace("(Clone)", "").Trim();
                poolManager.ReturnSegmentToPool(tag, oldestSegment);
                activeSegments.Dequeue();
            }
        }
    }

    void SpawnSegment()
    {
        // 1. Choose the base segment type.
        // 2. Choose a random group count between groupMinCount and groupMaxCount (inclusive).

        string chosenTag = "";
        int groupCount = Random.Range(groupMinCount, groupMaxCount + 1);
        List<string> availableTags = new List<string>(segmentTags);
        
        if (firstSegment)
        {
            firstSegment = false;
            chosenTag = "SafeSegment";
            groupCount = 6;
        }
        else
        {
            do
            {
                int index = Random.Range(0, availableTags.Count);
                chosenTag = availableTags[index];
            } 
            while(chosenTag.Equals(previousSegmentTag));
            
            if (chosenTag.Equals("SafeSegment"))
            {
                groupCount = 2;
            }
        }
        
        previousSegmentTag = chosenTag;
    
        // 3. Spawn the group of segments:
        // If only one segment is to be spawned, use the base name.
        // Otherwise, the first gets "Start", the last gets "End", and the ones in between get "Mid".
        for (int i = 0; i < groupCount; i++)
        {
            string finalTag;
            if (groupCount == 1)
            {
                finalTag = chosenTag;
            }
            else if (i == 0)
            {
                finalTag = chosenTag + "Start";
            }
            else if (i == groupCount - 1)
            {
                finalTag = chosenTag + "End";
            }
            else
            {
                finalTag = chosenTag + "Mid";
            }
        
            // Spawn each segment using a helper method.
            SpawnSingleSegment(finalTag);
        }
    }

    // Helper method to spawn a single segment with the given tag.
    private void SpawnSingleSegment(string tag)
    {
        // Position the new segment relative to the player's x position.
        Vector3 spawnPosition = new Vector3(player.transform.position.x, 0, nextSegmentZ);
        GameObject newSegment = poolManager.GetSegmentFromPool(tag, spawnPosition, Quaternion.identity);
        if (newSegment != null)
        {
            activeSegments.Enqueue(newSegment);
            Segment seg = newSegment.GetComponent<Segment>();
            float zLength = seg != null ? seg.GetZLength() : 10f;
            nextSegmentZ += zLength;
        }
    }

    // IGameStateListener implementation:
    public void OnGameStart()
    {
        Debug.Log("Game started");
        isGameActive = true;

        // Reset generator state.
        nextSegmentZ = worldStartZ;
        firstSegment = true;
        previousSegmentTag = "";
        activeSegments.Clear();

        // Reactivate the player.
        if (player != null)
            player.gameObject.SetActive(true);

        
        SpawnSingleSegment(safeZoneTag+"Start");
        // Spawn a constant number of safe zones.
        for (int i = 0; i < initialSafeZoneCount-2; i++)
        {
            SpawnSingleSegment(safeZoneTag+"Mid");
        }
        SpawnSingleSegment(safeZoneTag+"End");
        
        // Then spawn the initial regular segments.
        for (int i = 0; i < initialSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    public void OnGameOver()
    {
        isGameActive = false;

        // Clean up active segments.
        while (activeSegments.Count > 0)
        {
            GameObject segmentObj = activeSegments.Dequeue();
            string tag = segmentObj.name.Replace("(Clone)", "").Trim();
            poolManager.ReturnSegmentToPool(tag, segmentObj);
        }

        // Deactivate the player.
        if (player != null)
            player.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        if (WorldManager.Instance != null)
            WorldManager.Instance.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (WorldManager.Instance != null)
            WorldManager.Instance.UnregisterListener(this);
    }
}

