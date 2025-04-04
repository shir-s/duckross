using Objects;
using UnityEngine;
using Pools;

namespace Segments
{
    public class SafeSegment : Segment
    {
        [SerializeField] private GameObject chickToPickPrefab;  // Assign your chick prefab in the Inspector.
        
        private Transform player;
        private float yOffset = 0.5f;        // How much above the segment to spawn the chick.
        private bool chickToPickSpawned = false; // Ensure chick is spawned only once per activation.
        private GameObject chickToPick;
        private float distanceFromPlayerToSpawnChick = 15;

        // Static variable to count how many chick objects have been spawned.
        public static int ChickSpawnCount { get; private set; } = 0;

        void Update()
        {
            if (player.position.z > transform.position.z + distanceFromPlayerToSpawnChick && !chickToPickSpawned)
            {
                SpawnChick();
            }
        }

        private void OnEnable()
        {
            // Instead of assigning player via Inspector, fetch it via singleton.
            if (player == null && PlayerController.Instance != null)
            {
                player = PlayerController.Instance.gameObject.transform;
            }

            chickToPickSpawned = false;
            if (!chickToPickSpawned && Random.value < 1f && transform.position.z > 2)
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
                // Pass the direction to the obstacle's behavior.
                ChickToSpawn behavior = chickToPick.GetComponent<ChickToSpawn>();
                if (behavior != null)
                {
                    // Set this RoadSegment as the spawner so the obstacle can return itself.
                    behavior.SetSpawner(this);
                }
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
        public override void GenerateObject() { }

        public override void ReturnOject(GameObject obj)
        {
            string tag = obj.name.Replace("(Clone)", "").Trim();
            ObjectPoolManager.Instance.ReturnObjectToPool(tag, obj);
            chickToPickSpawned = false;
        }
    }
}
