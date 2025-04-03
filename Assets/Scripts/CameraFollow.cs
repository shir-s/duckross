using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player; // Drag your player here in the Inspector.
    [SerializeField] private Vector3 offset;     // Set this offset to position the camera relative to the player.

    void LateUpdate()
    {
        if(player != null)
        {
            transform.position = player.position + offset;
        }
    }
}