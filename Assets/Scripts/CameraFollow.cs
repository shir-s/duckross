using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // Drag your player here in the Inspector.
    [SerializeField] private Vector3 offset;     // Set this offset to position the camera relative to the player.
    private float minX = -1f;    // Minimum x boundary.
    private float maxX = 1f;     // Maximum x boundary.

    void LateUpdate()
    {
        if(player != null)
        {
            Vector3 desiredPosition = player.position + offset;
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            transform.position = desiredPosition;
        }
    }
}