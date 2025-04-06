using System.Collections;
using System.Collections.Generic;
using Managers;
using Objects;
using Pools;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static int ChickCount { get; private set; }
    [SerializeField] private float jumpDuration = 0.2f;  
    [SerializeField] private float jumpDistance = 2f;      
    public List<GameObject> chicks = new List<GameObject>();

    // Animation handling
    private Animator animator;
    private Animator childAnimator;

    private Vector3 startPosition = new Vector3(0, 1, 0);
    private Vector3 targetPosition;
    private bool isJumping = false;
    private List<Vector3> previousPositions = new List<Vector3>();

    // Variables for finish segment logic.
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
        // Automatically assign the animator from the child if it's not already set.
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        if (childAnimator == null)
        {
            childAnimator = GetComponentInChildren<Animator>();
        }
    }
    
    private void Update()
    {
        /*if(animator != null)
        {
            bool idleState = !isJumping;
            animator.SetBool("IsIdle", idleState);
            Debug.Log("IsIdle set to: " + idleState);
        }*/

        // Reset canPassFinish once the player has moved past the finish trigger point.
        if (canPassFinish && transform.position.z > finishPassStartZ + 10)
        {
            canPassFinish = false;
            finishPassStartZ = 0;
        }
    
        // If the player is on a finish segment and not allowed to pass, push them back.
        if (finishPassStartZ != 0 && !canPassFinish && transform.position.z > finishPassStartZ)
        {
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
    
        // Handle directional input from arrow keys or WASD, plus space for repeating last direction.
        if (!isJumping)
        {
            Vector3 jumpDirection = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                jumpDirection = Vector3.forward;
            else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                jumpDirection = Vector3.back;
            else if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                jumpDirection = Vector3.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                jumpDirection = Vector3.right;
            else if (Input.GetKeyDown(KeyCode.Space))
            {
                if (prevDirection != Vector3.zero)
                    jumpDirection = prevDirection;
            }

            if (jumpDirection != Vector3.zero && jumpDirection != -prevDirection)
            {
                // Update rotation based on direction.
                if (jumpDirection == Vector3.forward)
                    transform.rotation = Quaternion.Euler(0, 0, 0);
                else if (jumpDirection == Vector3.back)
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                else if (jumpDirection == Vector3.left)
                    transform.rotation = Quaternion.Euler(0, -90, 0);
                else if (jumpDirection == Vector3.right)
                    transform.rotation = Quaternion.Euler(0, 90, 0);

                prevDirection = jumpDirection;
                StartCoroutine(Jump(jumpDirection));
            }
        }
    }
    
    private IEnumerator Jump(Vector3 direction)
    {
        // play jump sound 
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

                // Calculate the direction the chick should face.
                Vector3 direction = previousPositions[i] - previousPositions[i + 1];
                if (direction != Vector3.zero)
                {
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    // Adjust rotation with an offset (example offset here, adjust as needed).
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

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeSegmentFinish"))
        {
            // Optionally set onFinishSegment false if needed.
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
            SoundManager.Instance.PlayCollectChick();
        }
        ChickCount = chicks.Count;
    }

    private void HandleCollision()
    {
        SoundManager.Instance.PlayCarHit();
        if (childAnimator != null)
        {
            Debug.Log("child animator enabled");
            childAnimator.enabled = false;
        }

        if (animator != null)
        {
            SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
            if (childSprite != null)
            {
                childSprite.enabled = false;
            }
            Debug.Log("animator enabled");
            animator.enabled = true;
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            gameObject.transform.position =  new Vector3(gameObject.transform.position.x, gameObject.transform.position.y+10, gameObject.transform.position.z);  ;
            animator.SetTrigger("Death");
        }
        GameOverByDeath();
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
        gameObject.transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        isJumping = false;
        transform.position = startPosition;
        targetPosition = transform.position;
        previousPositions.Clear();
        previousPositions.Add(transform.position);
        finishPassStartZ = 0;
        canPassFinish = false;
        prevFinish = 0;
        prevDirection = Vector3.forward;
        
        SpriteRenderer childSprite = GetComponentInChildren<SpriteRenderer>();
        if (childSprite != null)
        {
            childSprite.enabled = false;
        }
        
        if (childAnimator != null)
        {
            childAnimator.enabled = true;
        }

        if (EventManager.Instance != null)
        {
            Debug.Log("ADD LISTENER");
            EventManager.Instance.OnChicksPassedFinishSegment += HandleChicksPassedFinishSegment;
            EventManager.Instance.OnGameOverEvent += GameOver;
        }
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
            canPassFinish = true;
            finishPassStartZ = transform.position.z;
            prevFinish = transform.position.z;
            ReturnChicksFromChick(chicks[^requiredChicks]);
        }
    }

    private void GameOverByDeath()
    {
        SoundManager.Instance.PlayGameOver();
        EventManager.Instance.triggerGameOverByDeath();
        GameOver();
    }

    private void GameOver()
    {
        if (chicks.Count > 0)
            ReturnChicksFromChick(chicks[0]);
    }
}
