using Segments;
using UnityEngine;

namespace Objects
{
    public class ObjectBehavior : MonoBehaviour
    {
        private float moveSpeed = 5f;
        // When the object travels this distance, it will be returned to the pool.
        private float distanceThreshold = 100f;

        private Vector3 moveDirection = Vector3.zero;
        private Vector3 initialPosition;   // Where the object spawned.
        private string objectTag;        // Tag for returning to pool.
        private Segment spawnerSegment;

        // Call this method to set the movement direction.
        public void Initialize(Vector3 direction, float speed)
        {
            moveSpeed = speed;
            /*Debug.Log($"Speed: {moveSpeed}, Distance: {distanceThreshold}");*/
            // Only consider the X component; ignore Y and Z.
            moveDirection = new Vector3(-direction.x, 0, 0);
            // Record the spawn position for later distance calculations.
            initialPosition = transform.position;
            // Set the object tag from the object's name (remove "(Clone)" if present).
            objectTag = gameObject.name.Replace("(Clone)", "").Trim();
        }


        void Update()
        {
            // Calculate the change in X position only.
            float deltaX = moveSpeed * Time.deltaTime * moveDirection.x;
            // Update the position, keeping Y and Z fixed.
            transform.position = new Vector3(transform.position.x + deltaX, initialPosition.y, initialPosition.z);

            // Calculate how far the object has traveled along the X-axis.
            float traveled = Mathf.Abs(transform.position.x - initialPosition.x);
        
            // When the traveled distance exceeds the threshold, return the object to the pool.
            if (traveled >= distanceThreshold)
            {
                Debug.LogWarning($"Object traveled: {traveled}");
                spawnerSegment.ReturnOject(gameObject);
            }
        }

        public void SetSpawner(Segment segment)
        {
            spawnerSegment = segment;
        }
    }
}