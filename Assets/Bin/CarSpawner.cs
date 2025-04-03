/*
using UnityEngine;
using System.Collections;

public class CarSpawner : MonoBehaviour, IObstacleSpawner
{
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private float spawnInterval = 2.0f; // Default interval; will be overwritten by ActivateSpawner.

    private Coroutine spawnCoroutine;

    // Activate the spawner to repeatedly spawn obstacles with the given interval and direction.
    public void ActivateSpawner(float interval, SpawnDirection direction)
    {
        spawnInterval = interval;
        // If there's an existing spawning coroutine, stop it.
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
        // Start the coroutine with the specified direction.
        spawnCoroutine = StartCoroutine(SpawnCoroutine(direction));
    }

    // Stops the spawning coroutine.
    public void DeactivateSpawner()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    // Coroutine that spawns obstacles repeatedly.
    private IEnumerator SpawnCoroutine(SpawnDirection direction)
    {
        while (true)
        {
            SpawnObstacle(direction);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    // Spawns a single obstacle, adjusting the x-offset based on the spawn direction.
    private void SpawnObstacle(SpawnDirection direction)
    {
        float xOffset = 0f;
        // If spawning to the left, force a negative x offset; if to the right, positive.
        if (direction == SpawnDirection.Left)
        {
            xOffset = -Random.Range(1f, 5f);
        }
        else if (direction == SpawnDirection.Right)
        {
            xOffset = Random.Range(1f, 5f);
        }

        // Randomize z offset within a range (adjust as needed).
        float zOffset = Random.Range(0, 10f);
        Vector3 spawnPos = new Vector3(xOffset, 0, zOffset);

        // Instantiate the car obstacle as a child of this spawner.
        Instantiate(carPrefab, transform.position + spawnPos, Quaternion.identity, transform);
    }
}
*/

