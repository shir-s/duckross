using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
    private float moveSpeed = 5f;
    // When the obstacle travels this distance, it will be returned to the pool.
    private float distanceThreshold = 20f;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 initialPosition;   // Where the obstacle spawned.
    private string obstacleTag;        // Tag for returning to pool.
    private Segment spawnerSegment;

    // Call this method to set the movement direction.
    public void Initialize(Vector3 direction, float speed, float distance)
    {
        moveSpeed = speed;
        distanceThreshold = distance;
        /*Debug.Log($"Speed: {moveSpeed}, Distance: {distanceThreshold}");*/
        // Only consider the X component; ignore Y and Z.
        moveDirection = new Vector3(-direction.x, 0, 0);
        // Record the spawn position for later distance calculations.
        initialPosition = transform.position;
        // Set the obstacle tag from the object's name (remove "(Clone)" if present).
        obstacleTag = gameObject.name.Replace("(Clone)", "").Trim();
    }


    void Update()
    {
        // Calculate the change in X position only.
        float deltaX = moveSpeed * Time.deltaTime * moveDirection.x;
        // Update the position, keeping Y and Z fixed.
        transform.position = new Vector3(transform.position.x + deltaX, initialPosition.y, initialPosition.z);

        // Calculate how far the obstacle has traveled along the X-axis.
        float traveled = Mathf.Abs(transform.position.x - initialPosition.x);
        
        // When the traveled distance exceeds the threshold, return the obstacle to the pool.
        if (traveled >= distanceThreshold)
        {
            spawnerSegment.ReturnObstacle(gameObject);
        }
    }

    public void SetSpawner(Segment segment)
    {
        spawnerSegment = segment;
    }
}