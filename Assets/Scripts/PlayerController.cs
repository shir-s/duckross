using System.Collections;
using System.Collections.Generic;
using Managers;
using Objects;
using Pools;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static int ChickCount { get; private set; }
    [SerializeField] private float jumpDuration = 0.2f;  
    [SerializeField] private float jumpDistance = 2f;      
    public List<GameObject> chicks = new List<GameObject>();
    
    private Vector3 startPosition = new Vector3(0, 1, 0);
    private Vector3 targetPosition;
    private bool isJumping = false;
    private List<Vector3> previousPositions = new List<Vector3>();

    // New variables for finish segment logic.
    private bool canPassFinish = false;
    private float finishPassStartZ = 0;
    private float rightBorder = 10;
    private float leftBorder = -10;
    private float prevFinish = 0;
    private Vector3 prevDirection = Vector3.forward;
    
    public static PlayerController Instance { get; private set; }
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // Optionally: DontDestroyOnLoad(gameObject);
    }
    
    private void Update()
    {
        // Reset canPassFinish once the player has moved past the finish trigger point.
        if (canPassFinish && transform.position.z > finishPassStartZ + 10 )
        {
            canPassFinish = false;
            finishPassStartZ = 0;
        }
    
        // If the player is on a finish segment, not allowed to pass, and has moved beyond the saved finish segment z,
        // check for the down arrow to push them back.
        if (finishPassStartZ != 0 && !canPassFinish && transform.position.z > finishPassStartZ)
        {
            // Push the player backward along z (adjust the magnitude as needed).
            transform.position += new Vector3(0, 0, -1f);
        }
        if (transform.position.z < prevFinish)
        {
            transform.position += new Vector3(0, 0, 1f);
        }
        if (transform.position.x > rightBorder)
        {
            transform.position += new Vector3(-1f, 0, 0);
        }
        if (transform.position.x < leftBorder)
        {
            transform.position += new Vector3(1f, 0, 0);
        }
    
        // Existing jump logic...
        if (!isJumping)
        {
            Vector3 jumpDirection = Vector3.zero;
        
            // Use arrow keys or WASD for directional input.
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                jumpDirection = Vector3.forward;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                jumpDirection = Vector3.back;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                jumpDirection = Vector3.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                jumpDirection = Vector3.right;
            // Space jumps in the last direction if available.
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (prevDirection != Vector3.zero)
                    jumpDirection = prevDirection;
            }

            if (jumpDirection != Vector3.zero && jumpDirection != -prevDirection)
            {
                // Update rotation based on the new direction.
                if (jumpDirection == Vector3.forward)
                    transform.rotation = Quaternion.Euler(90, 0, 0);
                else if (jumpDirection == Vector3.back)
                    transform.rotation = Quaternion.Euler(90, 180, 0);
                else if (jumpDirection == Vector3.left)
                    transform.rotation = Quaternion.Euler(90, -90, 0);
                else if (jumpDirection == Vector3.right)
                    transform.rotation = Quaternion.Euler(90, 90, 0);

                prevDirection = jumpDirection;
                StartCoroutine(Jump(jumpDirection));
            }
        }
    }
    private IEnumerator Jump(Vector3 direction)
    {
        isJumping = true;
        Vector3 start = transform.position;
        targetPosition = start + direction.normalized * jumpDistance;
        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            transform.position = Vector3.Lerp(start, targetPosition, elapsed / jumpDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        previousPositions.Insert(0, transform.position);
        while (previousPositions.Count > chicks.Count + 1)
        {
            previousPositions.RemoveAt(previousPositions.Count - 1);
        }
        MoveChicks();
        isJumping = false;
    }

    private void MoveChicks()
    {
        for (int i = 0; i < chicks.Count; i++)
        {
            if (i + 1 < previousPositions.Count)
            {
                chicks[i].transform.position = previousPositions[i + 1];
                
                // Calculate the direction the chick should look toward (from its current position to its previous position)
                Vector3 direction = previousPositions[i] - previousPositions[i + 1];
                if(direction != Vector3.zero)
                {
                    // Create a rotation that looks in that direction.
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    // Adjust the rotation by the constant offset (around the Y axis, for example).
                    Quaternion adjustedRotation = lookRotation * Quaternion.Euler(90, 0, 0);
                    chicks[i].transform.rotation = adjustedRotation;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ChickToPick"))
        {
            AddChick();
        }
        else if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
        {
            HandleCollision();
        }
        else if (other.CompareTag("SafeSegmentFinish"))
        {
            // Save the finish segment's z coordinate from the collided object.
            finishPassStartZ = other.transform.position.z;
            if (!canPassFinish)
            {
                Debug.Log("Cannot pass finish yet.");
            }
        }
    }

    private void AddChick()
    {
        GameObject chick = ObjectPoolManager.Instance.GetObjectFromPool("Chick", transform.position);
        if (chick != null)
        {
            chicks.Add(chick);
            chick.tag = "Chick";
            chick.AddComponent<ChickController>();
        }
        ChickCount = chicks.Count;
    }

    private void HandleCollision()
    {
        /*if (chicks.Count > 0)
            ReturnChicksFromChick(chicks[0]);
        else*/
        GameOver();
    }

    public void ReturnChicksFromChick(GameObject hitChick)
    {
        int hitIndex = chicks.IndexOf(hitChick);
        if (hitIndex == -1) return;
        for (int i = hitIndex; i < chicks.Count; i++)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool("Chick", chicks[i]);
        }
        chicks.RemoveRange(hitIndex, chicks.Count - hitIndex);
        ChickCount = chicks.Count;
    }

    private void OnEnable()
    {
        // Subscribe to event.
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnGameStartEvent += HandleGameStart;
            EventManager.Instance.OnChicksPassedFinishSegment += HandleChicksPassedFinishSegment;
        }
    }

    private void HandleGameStart()
    {
        isJumping = false;
        transform.position = startPosition;
        targetPosition = transform.position;
        previousPositions.Clear();
        previousPositions.Add(transform.position);
        finishPassStartZ = 0;
        canPassFinish = false;
        prevFinish = 0;
        prevDirection = Vector3.forward;

        if (chicks.Count > 0)
            ReturnChicksFromChick(chicks[0]);
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnChicksPassedFinishSegment -= HandleChicksPassedFinishSegment;
        }
    }

    private void HandleChicksPassedFinishSegment(int requiredChicks)
    {
        Debug.Log($"HandleChicksPassedFinishSegment {requiredChicks}");
        if (prevFinish + 10 < transform.position.z)
        {
            // When enough chicks have passed the finish segment, set canPassFinish true and store current z.
            canPassFinish = true;
            finishPassStartZ = transform.position.z;
            prevFinish = transform.position.z;
            ReturnChicksFromChick(chicks[^requiredChicks]);
        }
    }

    private void GameOver()
    {
        if (chicks.Count > 0)
            ReturnChicksFromChick(chicks[0]);
        EventManager.Instance.EndGame();
    }
}
