using UnityEngine;
using Managers;

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
                SoundManager.Instance.PlayCarHit();
                _player.ReturnChicksFromChick(gameObject);
            }
        }
    }
}