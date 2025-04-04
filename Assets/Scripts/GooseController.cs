using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Pools;
using UnityEngine;

public class GooseController : MonoBehaviour
{
    [SerializeField] private float jumpDuration = 0.2f;  // Duration of the jump.
    [SerializeField] private float jumpDistance = 2f;      // Distance per jump.
    public List<GameObject> chicks = new List<GameObject>();
    
    private Vector3 startPosition = new Vector3(0, 1, 0);

    private Vector3 targetPosition;
    private bool isJumping = false;
    private List<Vector3> previousPositions = new List<Vector3>();

    void Update()
    {
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
        // Each chick follows a previous recorded position.
        for (int i = 0; i < chicks.Count; i++)
        {
            if (i + 1 < previousPositions.Count)
            {
                chicks[i].transform.position = previousPositions[i + 1];
            }
        }
    }

    // When the goose collides with something.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("ChickToPick"))
        {
            // When colliding with a fruit, add a chick from the pool.
            AddChick();
        }
        else if (other.CompareTag("Car") || other.CompareTag("Truck"))
        {
            HandleCollision();
        }
    }

    // Retrieves a chick from the pool and adds it to the chicks list.
    private void AddChick()
    {
        GameObject chick = ObjectPoolManager.Instance.GetObjectFromPool("Chick", transform.position);
        if (chick != null)
        {
            chicks.Add(chick);
            chick.tag = "Chick";  // Ensure its tag is set correctly.
            // Add the ChickController so that the chick can report collisions.
            chick.AddComponent<ChickController>();
        }
    }

    // Called when the goose collides with a car/truck.
    private void HandleCollision()
    {
        if (chicks.Count > 0)
            ReturnChicksFromIndex(chicks[0]);  // In this example, remove all chicks.
        else
            GameOver();
    }

    // Called by a chick to remove it and all subsequent chicks.
    public void ReturnChicksFromIndex(GameObject hitChick)
    {
        int hitIndex = chicks.IndexOf(hitChick);
        if (hitIndex == -1)
            return;

        for (int i = hitIndex; i < chicks.Count; i++)
        {
            ObjectPoolManager.Instance.ReturnObjectToPool("Chick", chicks[i]);
        }
        chicks.RemoveRange(hitIndex, chicks.Count - hitIndex);
    }

    private void OnEnable()
    {
        isJumping = false;
        transform.position = startPosition;
        targetPosition = transform.position;
        previousPositions.Add(transform.position);
    }

    private void OnDisable()
    {
        /*// but just be invisible, disable its SpriteRenderer (or Renderer):
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = false;
        }*/
    }


    private void GameOver()
    {
        EventManager.Instance.EndGame();
    }
}
