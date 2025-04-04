using System.Collections;
using System.Collections.Generic;
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
    private bool onFinishSegment = false;
    private bool canPassFinish = false;
    private float finishPassStartZ = 0;

    private void Update()
    {
        // Reset canPassFinish once the player has moved past the finish trigger point.
        if (canPassFinish && transform.position.z > finishPassStartZ)
        {
            canPassFinish = false;
            Debug.Log("Player moved beyond finish trigger, resetting canPassFinish.");
        }
    
        // If the player is on a finish segment, not allowed to pass, and has moved beyond the saved finish segment z,
        // check for the down arrow to push them back.
        if (finishPassStartZ != 0 && !canPassFinish && transform.position.z > finishPassStartZ)
        {
            // Push the player backward along z (adjust the magnitude as needed).
            transform.position += new Vector3(0, 0, -1f);
            Debug.Log("Player bush off triggered.");
        }
    
        // Existing jump logic...
        if (!isJumping)
        {
            Vector3 jumpDirection = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                jumpDirection = Vector3.forward;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                jumpDirection = Vector3.back;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                jumpDirection = Vector3.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                jumpDirection = Vector3.right;

            if (jumpDirection != Vector3.zero)
            {
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
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ChickToPick"))
        {
            AddChick();
        }
        else if (other.CompareTag("Car") || other.CompareTag("Truck"))
        {
            HandleCollision();
        }
        else if (other.CompareTag("SafeSegmentFinish"))
        {
            onFinishSegment = true;
            // Save the finish segment's z coordinate from the collided object.
            finishPassStartZ = other.transform.position.z;
            Debug.Log("Player collided with SafeSegmentFinish. Saved finish segment z: " + finishPassStartZ);
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
            onFinishSegment = false;
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
        if (chicks.Count > 0)
            ReturnChicksFromIndex(chicks[0]);
        else
            GameOver();
    }

    public void ReturnChicksFromIndex(GameObject hitChick)
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
        isJumping = false;
        transform.position = startPosition;
        targetPosition = transform.position;
        previousPositions.Clear();
        previousPositions.Add(transform.position);

        // Subscribe to event.
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnChicksPassedFinishSegment += HandleChicksPassedFinishSegment;
        }
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnChicksPassedFinishSegment -= HandleChicksPassedFinishSegment;
        }
    }

    private void HandleChicksPassedFinishSegment()
    {
        // When enough chicks have passed the finish segment, set canPassFinish true and store current z.
        canPassFinish = true;
        finishPassStartZ = transform.position.z;
        Debug.Log("Chicks passed finish segment. canPassFinish set to true.");
    }

    private void GameOver()
    {
        EventManager.Instance.EndGame();
    }
}
