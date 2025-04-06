using System.Collections;
using System.Collections.Generic;
using Objects;
using Pools;
using UnityEngine;

namespace Segments
{

    public class ObstacleSegment : Segment
    {
        private enum SpawnDirection { Left, Right }

        // List of allowed obstacle prefabs (cars/trucks, logs/boats, etc.).
        [SerializeField] private List<GameObject> allowedObstaclePrefabs;

        // Chosen spawn direction.
        private SpawnDirection spawnDirection;

        // Coroutine reference.
        private Coroutine spawnCoroutine;
        private GameObject obstaclePrefab;
        // Set 1: For obstacles tagged "obstacle1".
        private float obstacleSpawnMinIntervalObj1 = 2f;
        private float obstacleSpawnMaxIntervalObj1 = 5f;
        private float obstacleSpeedMinObj1 = 3f;
        private float obstacleSpeedMaxObj1 = 6f;

        // Set 2: For obstacles tagged "obstacle2".
        private float obstacleSpawnMinIntervalObj2 = 5f;
        private float obstacleSpawnMaxIntervalObj2 = 8f;
        private float obstacleSpeedMinObj2 = 1f;
        private float obstacleSpeedMaxObj2 = 3f;
        private float obstacleSpawnInterval;
        private float obstacleSpeed;

        private float distanceThreshold;
    
        // List to track all spawned obstacles.
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        private void OnEnable()
        {
            // Check if allowedObstaclePrefabs is assigned.
            if (allowedObstaclePrefabs == null || allowedObstaclePrefabs.Count == 0)
            {
                Debug.LogWarning("No allowed obstacle prefabs assigned for " + gameObject.name);
                return;
            }
            
            // Choose a random obstacle prefab.
            int index = Random.Range(0, allowedObstaclePrefabs.Count);
            obstaclePrefab = allowedObstaclePrefabs[index];

            // Decide which set of values to use based on the prefab's tag.
            if (obstaclePrefab.CompareTag("Obstacle1"))
            {
                obstacleSpeed = Random.Range(obstacleSpeedMinObj1, obstacleSpeedMaxObj1);
                obstacleSpawnInterval = Mathf.Lerp(obstacleSpawnMinIntervalObj1, obstacleSpawnMaxIntervalObj1,
                    (obstacleSpeed - obstacleSpeedMinObj1) / (obstacleSpeedMaxObj1 - obstacleSpeedMinObj1));
            }
            else if (obstaclePrefab.CompareTag("Obstacle2"))
            {
                obstacleSpeed = Random.Range(obstacleSpeedMinObj2, obstacleSpeedMaxObj2);
                obstacleSpawnInterval = Mathf.Lerp(obstacleSpawnMinIntervalObj2, obstacleSpawnMaxIntervalObj2,
                    (obstacleSpeed - obstacleSpeedMinObj2) / (obstacleSpeedMaxObj2 - obstacleSpeedMinObj2));
            }
            else
            {
                // Fallback: use set 1 values.
                obstacleSpeed = Random.Range(obstacleSpeedMinObj1, obstacleSpeedMaxObj1);
                obstacleSpawnInterval = Mathf.Lerp(obstacleSpawnMinIntervalObj1, obstacleSpawnMaxIntervalObj1,
                    (obstacleSpeed - obstacleSpeedMinObj1) / (obstacleSpeedMaxObj1 - obstacleSpeedMinObj1));
            }

            // Set distance threshold.
            distanceThreshold = transform.localScale.x * 3;

            // Choose a random spawn direction.
            spawnDirection = (Random.value < 0.5f) ? SpawnDirection.Left : SpawnDirection.Right;

            // Start the spawning coroutine.
            spawnCoroutine = StartCoroutine(SpawnObstaclesRoutine());
        }
    
        private void OnDisable()
        {
            // Stop the spawn coroutine.
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
    
            // Return all spawned obstacles.
            foreach (GameObject obstacle in new List<GameObject>(spawnedObstacles))
            {
                if (obstacle != null)
                {
                    ReturnOject(obstacle);
                }
            }
            spawnedObstacles.Clear();
        }
    
        // Spawning coroutine.
        private IEnumerator SpawnObstaclesRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(obstacleSpawnInterval);
                GenerateObject();
            }
        }
    
        // Calculate top Y of the segment.
        private float GetTopY()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return transform.position.y;
            float topY = renderers[0].bounds.max.y;
            for (int i = 1; i < renderers.Length; i++)
            {
                topY = Mathf.Max(topY, renderers[i].bounds.max.y);
            }
            return topY;
        }
    
        // Generates one obstacle from the pool.
        public override void GenerateObject()
        {
            if (allowedObstaclePrefabs == null || allowedObstaclePrefabs.Count == 0)
            {
                Debug.LogWarning("No obstacle prefabs assigned for this segment.");
                return;
            }

            // Calculate spawn position.
            Vector3 spawnPosition = transform.position;
            float xLength = GetXLength();
            float xOffset = xLength * 0.5f;
            if (spawnDirection == SpawnDirection.Left)
                spawnPosition.x -= (xOffset - 2);
            else
                spawnPosition.x += (xOffset - 2);
            spawnPosition.y = GetTopY() + 0.35f;

            // Retrieve the obstacle from the pool.
            string obstacleTag = obstaclePrefab.name;
            GameObject obstacle = ObjectPoolManager.Instance.GetObjectFromPool(obstacleTag, spawnPosition);
            if (obstacle != null)
            {
                var leftRotation = Quaternion.Euler(90, -90, 0);
                var rightRotation = Quaternion.Euler(90, 90, 0);
                Vector3 obstacleDirection = (spawnDirection == SpawnDirection.Left) ? Vector3.left : Vector3.right;
                var rotation = (spawnDirection == SpawnDirection.Left) ? leftRotation : rightRotation;
                obstacle.transform.rotation = rotation;
                ObjectBehavior behavior = obstacle.GetComponent<ObjectBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(obstacleDirection, obstacleSpeed);
                    behavior.SetSpawner(this);
                    
                }
                spawnedObstacles.Add(obstacle);
            }
        }
    
        // Returns an object to the pool.
        public override void ReturnOject(GameObject obj)
        {
            string tag = obj.name.Replace("(Clone)", "").Trim();
            if (spawnedObstacles.Contains(obj))
            {
                spawnedObstacles.Remove(obj);
            }
            ObjectPoolManager.Instance.ReturnObjectToPool(tag, obj);
        }
    }
}
