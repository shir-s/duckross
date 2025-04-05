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
        [SerializeField] private float obstacleSpawnMinInterval = 2f;
        [SerializeField] private float obstacleSpawnMaxInterval = 6f;
        private float obstacleSpawnInterval;

        [SerializeField] private float obstacleSpeedMin = 1f;
        [SerializeField] private float obstacleSpeedMax = 3f;
        private float obstacleSpeed;

        private float distanceThreshold;
    
        // List to track all spawned obstacles.
        private List<GameObject> spawnedObstacles = new List<GameObject>();

        private void OnEnable()
        {
            // Set obstacle speed and threshold.
            obstacleSpeed = Random.Range(obstacleSpeedMin, obstacleSpeedMax);
            distanceThreshold = transform.localScale.x * 3;

            // Choose spawn direction.
            spawnDirection = (Random.value < 0.5f) ? SpawnDirection.Left : SpawnDirection.Right;

            // Randomly choose an obstacle prefab from the list.
            int index = Random.Range(0, allowedObstaclePrefabs.Count);
            obstaclePrefab = allowedObstaclePrefabs[index];

            // Calculate spawn interval based on obstacle speed.
            obstacleSpawnInterval = Mathf.Lerp(obstacleSpawnMinInterval, obstacleSpawnMaxInterval,
                (obstacleSpeed - obstacleSpeedMin) / (obstacleSpeedMax - obstacleSpeedMin));

            // Start spawning obstacles.
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
                Vector3 obstacleDirection = (spawnDirection == SpawnDirection.Left) ? Vector3.left : Vector3.right;
                ObjectBehavior behavior = obstacle.GetComponent<ObjectBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(obstacleDirection, obstacleSpeed, distanceThreshold);
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
