using UnityEngine;

public class ChickController : MonoBehaviour
{
    private GooseController _goose;

    private void Start()
    {
        _goose = FindObjectOfType<GooseController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") || other.CompareTag("Truck"))
        {
            _goose.ReturnChicksFromIndex(gameObject);
        }
    }
}