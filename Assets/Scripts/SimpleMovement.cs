using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;

    void Update()
    {
        Vector3 move = Vector3.zero;

        // Horizontal & Forward/Back movement using WASD.
        if (Input.GetKey(KeyCode.W))
            move += transform.forward;
        if (Input.GetKey(KeyCode.S))
            move -= transform.forward;
        if (Input.GetKey(KeyCode.A))
            move -= transform.right;
        if (Input.GetKey(KeyCode.D))
            move += transform.right;

        // Vertical movement: Space is up, Shift (left or right) is down.
        if (Input.GetKey(KeyCode.Space))
            move += transform.up * verticalSpeed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            move -= transform.up * verticalSpeed;

        // Normalize the horizontal movement to ensure consistent speed in all directions.
        Vector3 horizontalMove = new Vector3(move.x, 0f, move.z).normalized * moveSpeed;

        // Vertical movement is applied separately.
        float verticalMove = move.y;

        // Combine both horizontal and vertical movement.
        Vector3 finalMove = new Vector3(horizontalMove.x, verticalMove, horizontalMove.z);

        // Apply the movement.
        transform.Translate(finalMove * Time.deltaTime, Space.World);
    }
}