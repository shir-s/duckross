using UnityEngine;

namespace Segments
{
    public class SafeSegment : Segment
    {
        [SerializeField] private GameObject fruitPrefab;  // Assign your fruit prefab in the Inspector.
        [SerializeField] private float yOffset = 5f;        // How much above the segment to spawn the fruit.

        private bool fruitSpawned = false; // Ensure fruit is spawned only once per activation.
        private GameObject fruit;

        private void OnEnable()
        {
            // Reset flag each time the segment is activated.
            fruitSpawned = false;

            // With 33% chance, spawn one fruit (if not already spawned).
            if (!fruitSpawned && Random.value < 0.33f && transform.position.z > 2)
            {
                SpawnFruit();
            }
        }

        private void SpawnFruit()
        {
            fruitSpawned = true; // Mark that we've spawned a fruit for this activation.

            // Get half the segment's width.
            float halfWidth = GetXLength() * 0.5f;
            // Randomize x offset within the segment's width.
            float xOffset = Random.Range(-halfWidth, halfWidth);

            // The fruit's spawn position:
            // - x: segment center + random offset within width.
            // - y: segment's y plus yOffset so it appears on top.
            // - z: exactly the same as the segment's z.
            Vector3 spawnPosition = new Vector3(
                transform.position.x + xOffset,
                transform.position.y + yOffset,
                transform.position.z
            );

            // Force x and z to be multiples of 2.
            spawnPosition.x = Mathf.Round(spawnPosition.x / 2f) * 2f;
            spawnPosition.z = Mathf.Round(spawnPosition.z / 2f) * 2f;

            fruit = Instantiate(fruitPrefab, spawnPosition, Quaternion.identity);
        }
    
        private void OnDisable()
        {
            // Return the fruit to the pool if one was spawned.
            if (fruit != null)
            {
                // Remove "(Clone)" from the name to match the pool key.
                string fruitTag = fruit.name.Replace("(Clone)", "").Trim();
                ObjectPoolManager.Instance.ReturnObjectToPool(fruitTag, fruit);
                fruit = null;
            }
            fruitSpawned = false;
        }
        // This segment type does not generate additional obstacles.
        public override void GenerateObstacles() { }
        public override void ReturnObstacle(GameObject obstacle) { }
    }
}