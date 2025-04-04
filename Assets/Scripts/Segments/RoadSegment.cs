using System.Collections;
using System.Collections.Generic;
using Objects;
using Pools;
using UnityEngine;

namespace Segments
{
    public enum SpawnDirection { Left, Right }

    public class RoadSegment : Segment
    {
        // List of allowed obstacle prefabs for this segment (e.g., Car, Truck, etc.)
        [SerializeField] private List<GameObject> allowedObstaclePrefabs;

        // Chosen spawn direction for obstacles.
        private SpawnDirection spawnDirection;

        // Reference to the Coroutine so it can be stopped later.
        private Coroutine spawnCoroutine;
        private GameObject obstaclePrefab;
        private float obstacleSpawnMinInterval = 2f;
        private float obstacleSpawnMaxInterval = 6f;
        private float obstacleSpawnInterval;

        private float obstacleSpeedMin = 1;
        private float obstacleSpeedMax = 3;
        private float obstacleSpeed;

        private float distanceThreshold;
    
        // List to track all obstacles spawned by this segment.
        private List<GameObject> spawnedObstacles = new List<GameObject>();
    

        private void OnEnable()
        {
            // Set obstacle speed and distance threshold.
            obstacleSpeed = Random.Range(obstacleSpeedMin, obstacleSpeedMax);
            distanceThreshold = transform.localScale.x * 3;
        
            // Choose a random spawn direction.
            spawnDirection = (Random.value < 0.5f) ? SpawnDirection.Left : SpawnDirection.Right;

            // Randomly choose one obstacle prefab from the list.
            int index = Random.Range(0, allowedObstaclePrefabs.Count);
            obstaclePrefab = allowedObstaclePrefabs[index];
            obstacleSpawnInterval = Random.Range(obstacleSpawnMinInterval, obstacleSpawnMaxInterval);
            
            // Start the regular spawning coroutine.
            spawnCoroutine = StartCoroutine(SpawnObstaclesRoutine());
        }
    
        private void OnDisable()
        {
            // Stop spawning obstacles.
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }
    
            // Iterate over a copy of the list to safely return obstacles.
            foreach (GameObject obstacle in new List<GameObject>(spawnedObstacles))
            {
                if (obstacle != null)
                {
                    ReturnOject(obstacle);
                }
            }
            spawnedObstacles.Clear();
        }

        // Coroutine that spawns obstacles repeatedly.
        private IEnumerator SpawnObstaclesRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(obstacleSpawnInterval);
                GenerateObject();
            }
        }

        // Helper method to calculate the top Y value of the segment.
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
    
        // GenerateObstacles spawns one obstacle using the pool.
        public override void GenerateObject()
        {
            if (allowedObstaclePrefabs == null || allowedObstaclePrefabs.Count == 0)
            {
                Debug.LogWarning("No obstacle prefabs assigned for this RoadSegment.");
                return;
            }

            // Calculate the spawn position.
            Vector3 spawnPosition = transform.position;
            float xLength = GetXLength();
            float xOffset = xLength * 0.5f;
            if (spawnDirection == SpawnDirection.Left)
                spawnPosition.x -= (xOffset - 2);
            else
                spawnPosition.x += (xOffset - 2);
            spawnPosition.y = GetTopY() + 0.35f;

            // Use the pool to retrieve the obstacle.
            string obstacleTag = obstaclePrefab.name;
            GameObject obstacle = ObjectPoolManager.Instance.GetObjectFromPool(obstacleTag, spawnPosition);

            if (obstacle != null)
            {
                // Determine movement direction.
                Vector3 obstacleDirection = (spawnDirection == SpawnDirection.Left) ? Vector3.left : Vector3.right;

                // Pass the direction to the obstacle's behavior.
                ObjectBehavior behavior = obstacle.GetComponent<ObjectBehavior>();
                if (behavior != null)
                {
                    behavior.Initialize(obstacleDirection, obstacleSpeed, distanceThreshold);
                    // Set this RoadSegment as the spawner so the obstacle can return itself.
                    behavior.SetSpawner(this);
                }
                spawnedObstacles.Add(obstacle);
            }
        }
    
        // Public method to centralize the return logic.
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