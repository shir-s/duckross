using UnityEngine;

namespace Objects
{
    public class ChickController : MonoBehaviour
    {
        private PlayerController _player;

        private void Start()
        {
            _player = FindObjectOfType<PlayerController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
            {
                _player.ReturnChicksFromChick(gameObject);
            }
        }
    }
}