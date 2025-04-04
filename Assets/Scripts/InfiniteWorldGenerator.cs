using System.Collections.Generic;
using Managers;
using Pools;
using Segments;
using UnityEngine;

public class InfiniteWorldGenerator : MonoBehaviour, IGameStateListener
{
    [SerializeField] private SegmentPoolManager poolManager;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 20f;   // When to spawn a new segment (based on player's z)
    [SerializeField] private float destroyDistanceBehind = 15f;  // When to remove an old segment (based on player's z)
    [SerializeField] private int initialSegmentCount = 5;        // Additional segments to spawn after initial safe zones.

    // List of base segment kinds (e.g., "RoadSegment", "FruitSegment", etc.).
    // For non-safe zones, your pool should contain prefabs with names like "RoadSegmentStart", "RoadSegmentMid", "RoadSegmentEnd", etc.
    [SerializeField] private List<string> segmentTags;

    // World start position on the z axis.
    [SerializeField] private float worldStartZ = 0f;

    // Group spawn settings for normal segments.
    [SerializeField] private int groupMinCount = 3;
    [SerializeField] private int groupMaxCount = 10;

    // Safe zone settings.
    private string safeZoneTag = "SafeSegment"; // Base tag for safe zone segments.
    [SerializeField] private int initialSafeZoneCount = 6; // Initial safe zone group count if desired.
    private float finishZoneInterval = 50f;  // Constant distance between finish zone groups.

    // Internal tracking for safe zone spawn.
    private float lastPlacedFinishZoneZ = 0f;
    private float nextFinishZoneZ;
    public float prevFinishZoneZ;

    // Instead of using player position as our base, we keep track of where the next segment should be placed.
    private float nextSegmentZ;
    
    // Queue to keep track of active segments.
    private Queue<GameObject> activeSegments = new Queue<GameObject>();

    // Counters to track segment types (for non-safe segments).
    private string previousSegmentTag = "";

    // Flag to control generator activity.
    private bool isGameActive = false;
    
    // Counter for safe segments spawned since the last finish.
    public static int safeSegmentCount = 0;
    
    // List to store finish segments.
    private List<GameObject> finishSegments = new List<GameObject>();

    void Update()
    {
        if (!isGameActive)
            return;

        // Spawn a new group of segments when the player's z plus spawnDistanceAhead exceeds nextSegmentZ.
        if (player.position.z + spawnDistanceAhead > nextSegmentZ)
        {
            SpawnSegment();
        }
        
        // Remove segments that have fallen too far behind the player,
        // but do NOT remove segments that are after (or at) the last finish zone.
        if (activeSegments.Count > 0)
        {
            GameObject oldestSegment = activeSegments.Peek();
            // Only delete if segment is far behind AND its z is less than the prevFinishZoneZ.
            if (oldestSegment.transform.position.z < prevFinishZoneZ &&
                player.position.z - oldestSegment.transform.position.z > destroyDistanceBehind)
            {
                string tag = oldestSegment.name.Replace("(Clone)", "").Trim();
                poolManager.ReturnSegmentToPool(tag, oldestSegment);
                activeSegments.Dequeue();
            }
        }
    }

    void SpawnSegment()
    {
        // First, check if it's time to spawn a safe zone group.
        if (nextSegmentZ >= lastPlacedFinishZoneZ + finishZoneInterval && !previousSegmentTag.Equals(safeZoneTag))
        {
            lastPlacedFinishZoneZ = nextSegmentZ;
            SpawnFinishZoneGroup();
            return;
        }
        
        // 1. Choose the base segment type.
        // 2. Choose a random group count between groupMinCount and groupMaxCount (inclusive).
        string chosenTag = "";
        int groupCount = Random.Range(groupMinCount, groupMaxCount + 1);
        List<string> availableTags = new List<string>(segmentTags);
        
        do
        {
            int index = Random.Range(0, availableTags.Count);
            chosenTag = availableTags[index];
        } while(chosenTag.Equals(previousSegmentTag));
            
        if (chosenTag.Equals("SafeSegment"))
        {
            groupCount = 2;
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

    // Spawns the safe zone group.
    void SpawnFinishZoneGroup()
    {
        // Safe zone group always consists of three segments:
        // SafeSegmentStart, SafeSegmentFinish, SafeSegmentEnd.
        SpawnSingleSegment(safeZoneTag + "Start");
        SpawnSingleSegment(safeZoneTag + "Finish");
        SpawnSingleSegment(safeZoneTag + "End");
        previousSegmentTag = safeZoneTag;
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
            float zLength = seg != null ? seg.GetZLength() : 2f;
            nextSegmentZ += zLength;
            
            // If this is a finish segment, add it to the finishSegments list.
            if (tag.EndsWith("Finish"))
            {
                finishSegments.Add(newSegment);
            }
        }
    }

    // This event handler updates the finish zone boundaries.
    // When triggered, it sets prevFinishZoneZ to nextFinishZoneZ,
    // and then updates nextFinishZoneZ to the z coordinate of the oldest finish segment in the list.
    private void HandleChicksPassedFinishSegment()
    {
        prevFinishZoneZ = nextFinishZoneZ;
        if (finishSegments.Count > 0)
        {
            // Assume the oldest finish segment is the first in the list.
            nextFinishZoneZ = finishSegments[0].transform.position.z;
            // Optionally, remove it from the list if you want to update once.
            finishSegments.RemoveAt(0);
        }
    }

    public void HandleGameRestart()
    {
        HandleGameOver();
        HandleGameStart();
    }

    public void HandleGameOver()
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

    public void HandleGameStart()
    {
        isGameActive = true;
        lastPlacedFinishZoneZ = 0f;
        // Reset generator state.
        nextSegmentZ = worldStartZ;
        prevFinishZoneZ = worldStartZ; // No finish yet.
        nextFinishZoneZ = worldStartZ + finishZoneInterval;
        previousSegmentTag = "";
        activeSegments.Clear();
        safeSegmentCount = 0;

        // Reactivate the player.
        if (player != null)
            player.gameObject.SetActive(true);

        // Optionally spawn initial safe zones (if desired, you can do this here).
        SpawnStartZoneGroup();

        // Then spawn the initial regular segments.
        for (int i = 0; i < initialSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    private void SpawnStartZoneGroup()
    {
        SpawnSingleSegment(safeZoneTag+"Start");
        // Spawn a constant number of safe zones.
        for (int i = 0; i < initialSafeZoneCount-2; i++)
        {
            SpawnSingleSegment(safeZoneTag+"Mid");
        }
        SpawnSingleSegment(safeZoneTag+"End");
        previousSegmentTag = safeZoneTag;
    }
    
    
    private void OnEnable()
    {
        // Subscribe to event.
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameStartEvent += HandleGameStart;
            EventManager.Instance.OnGameOverEvent += HandleGameOver;
            EventManager.Instance.OnGameRestartEvent += HandleGameRestart;
            EventManager.Instance.OnChicksPassedFinishSegment += HandleChicksPassedFinishSegment;
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameStartEvent -= HandleGameStart;
            EventManager.Instance.OnGameOverEvent -= HandleGameOver;
            EventManager.Instance.OnGameRestartEvent -= HandleGameRestart;
            EventManager.Instance.OnChicksPassedFinishSegment -= HandleChicksPassedFinishSegment;
        }
    }
}


