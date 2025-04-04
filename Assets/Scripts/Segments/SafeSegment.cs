using UnityEngine;
using Pools;

namespace Segments
{
    public class SafeSegment : Segment
    {
        [SerializeField] private GameObject chickToPickPrefab;  // Assign your chick prefab in the Inspector.
        
        private float yOffset = 0.5f;        // How much above the segment to spawn the chick.
        private bool chickToPickSpawned = false; // Ensure chick is spawned only once per activation.
        private GameObject chickToPick;

        // Static variable to count how many chick objects have been spawned.
        public static int ChickSpawnCount { get; private set; } = 0;

        private void OnEnable()
        {
            // Reset flag each time the segment is activated.
            chickToPickSpawned = false;

            // With 33% chance, spawn one chick (if not already spawned).
            if (!chickToPickSpawned && Random.value < 0.33f && transform.position.z > 2)
            {
                SpawnChick();
            }
        }

        private void SpawnChick()
        {
            chickToPickSpawned = true; // Mark that we've spawned a chick for this activation.

            // Get half the segment's width.
            float halfWidth = GetXLength() * 0.3f;
            // Randomize x offset within the segment's width.
            float xOffset = Random.Range(-halfWidth, halfWidth);

            // The chick's spawn position:
            // - x: segment center + random offset within width.
            // - y: segment's y plus yOffset so it appears on top.
            // - z: exactly the same as the segment's z.
            Vector3 spawnPosition = new Vector3(
                transform.position.x + xOffset,
                transform.position.y + yOffset,
                transform.position.z
            );

            // Force x and z to be multiples of 2.
            spawnPosition.x = Mathf.Round((spawnPosition.x) / 2f) * 2f;
            spawnPosition.z = Mathf.Round((spawnPosition.z) / 2f) * 2f;

            // Use the pool instead of instantiating a new object.
            string chickTag = chickToPickPrefab.name;
            chickToPick = ObjectPoolManager.Instance.GetObjectFromPool(chickTag, spawnPosition);
            if (chickToPick != null)
            {
                ChickSpawnCount++;
            }
        }
    
        private void OnDisable()
        {
            // Return the chick to the pool if one was spawned.
            if (chickToPick != null)
            {
                string chickTag = chickToPick.name.Replace("(Clone)", "").Trim();
                ObjectPoolManager.Instance.ReturnObjectToPool(chickTag, chickToPick);
                chickToPick = null;
                ChickSpawnCount--;
            }
            chickToPickSpawned = false;
        }

        // This segment type does not generate additional obstacles.
        public override void GenerateObstacles() { }
        public override void ReturnObstacle(GameObject obstacle) { }
    }
}
