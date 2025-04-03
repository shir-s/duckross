using System.Collections.Generic;
using UnityEngine;

public class InfiniteWorldGenerator : MonoBehaviour, IGameStateListener
{
    [SerializeField] private SegmentPoolManager poolManager;
    [SerializeField] private Transform player;
    [SerializeField] private float spawnDistanceAhead = 20f;   // When to spawn a new segment (based on player's z)
    [SerializeField] private float destroyDistanceBehind = 15f;  // When to remove an old segment (based on player's z)
    [SerializeField] private int initialSegmentCount = 5;

    // List of available segment tags (should match tags defined in your pool manager)
    [SerializeField] private List<string> segmentTags;

    // World start position on the z axis. New segments will be placed relative to this.
    [SerializeField] private float worldStartZ = 0f;

    // Instead of using player position as our base, we keep track of where the next segment should be placed.
    private float nextSegmentZ;
    
    // Queue to keep track of active segments.
    private Queue<GameObject> activeSegments = new Queue<GameObject>();

    // Base values for consecutive non-fruit segments.
    [SerializeField] private int baseMinConsecutiveNonFruit = 2;
    [SerializeField] private int baseMaxConsecutiveNonFruit = 4;

    // These values will grow as the player moves forward.
    private int minConsecutiveNonFruit;
    private int maxConsecutiveNonFruit;

    // Optional: multiplier to adjust the logarithmic growth rate.
    [SerializeField] private float logMultiplier = 1f;

    // Counters to track segment types.
    private int currentConsecutiveNonFruitCount = 0;
    private string previousNonFruitTag = "";
    private string previousSegmentTag = "";
    private bool firstSegment;

    // Flag to control generator activity.
    private bool isGameActive = false;

    void Update()
    {
        if (!isGameActive)
            return;

        // Update the maximum consecutive non-fruit threshold using logarithmic growth.
        int additional = (int)(logMultiplier * Mathf.Log(player.position.z + 1));
        maxConsecutiveNonFruit = baseMaxConsecutiveNonFruit + additional;

        // Spawn a new segment when the player's z plus spawnDistanceAhead exceeds nextSegmentZ.
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
        string chosenTag = "";
        int spawnAmount = 1;

        // Create a temporary list of available tags.
        List<string> availableTags = new List<string>(segmentTags);

        // Check if the previous segment was non-fruit.
        if (firstSegment)
        {
            chosenTag = "FruitSegment";
        }
        else if (!previousSegmentTag.Equals("FruitSegment"))
        {
            if (currentConsecutiveNonFruitCount < minConsecutiveNonFruit && !string.IsNullOrEmpty(previousNonFruitTag))
            {
                chosenTag = previousNonFruitTag;
            }
            else if (currentConsecutiveNonFruitCount >= maxConsecutiveNonFruit)
            {
                chosenTag = "FruitSegment";
            }
            else
            {
                int index = Random.Range(0, availableTags.Count);
                chosenTag = availableTags[index];
            }
        }
        else
        {
            // If the previous segment was a FruitSegment, remove it from available choices.
            availableTags.Remove("FruitSegment");
            int index = Random.Range(0, availableTags.Count);
            chosenTag = availableTags[index];
        }

        // Update counters.
        if (chosenTag.Equals("FruitSegment"))
        {
            currentConsecutiveNonFruitCount = 0;
            previousNonFruitTag = "";
            if (firstSegment)
            {
                spawnAmount = 5;
                firstSegment = false;
            }
            spawnAmount++;
        }
        else
        {
            if (previousNonFruitTag.Equals(chosenTag))
            {
                currentConsecutiveNonFruitCount++;
            }
            else
            {
                previousNonFruitTag = chosenTag;
                currentConsecutiveNonFruitCount = 1;
            }
        }
        previousSegmentTag = chosenTag;

        // Spawn the segments.
        for (int i = 0; i < spawnAmount; i++)
        {
            SpawnSingleSegment(chosenTag);
        }
    }

    // Helper method that spawns one segment with the given tag.
    private void SpawnSingleSegment(string tag)
    {
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
        minConsecutiveNonFruit = baseMinConsecutiveNonFruit;
        maxConsecutiveNonFruit = baseMaxConsecutiveNonFruit;
        firstSegment = true;
        currentConsecutiveNonFruitCount = 0;
        previousNonFruitTag = "";
        previousSegmentTag = "";
        activeSegments.Clear();

        // Reactivate the player.
        if (player != null)
        {
            player.gameObject.SetActive(true);
        }

        // Spawn the initial segments.
        for (int i = 0; i < initialSegmentCount; i++)
        {
            SpawnSegment();
        }
    }

    public void OnGameOver()
    {
        isGameActive = false;

        // Optionally, clean up active segments.
        while (activeSegments.Count > 0)
        {
            GameObject segmentObj = activeSegments.Dequeue();
            string tag = segmentObj.name.Replace("(Clone)", "").Trim();
            poolManager.ReturnSegmentToPool(tag, segmentObj);
        }

        // Deactivate the player.
        if (player != null)
        {
            player.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        Debug.Log("Infinite World Generator");
        if (WorldManager.Instance != null)
        {
            Debug.Log("Infinite World Generator1");
            WorldManager.Instance.RegisterListener(this);
        }
    }

    private void OnDisable()
    {
        if (WorldManager.Instance != null)
        {
            WorldManager.Instance.UnregisterListener(this);
        }
    }
}
