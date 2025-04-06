using System;
using UnityEngine;
using Managers;

namespace Objects
{
    public class ChickController : MonoBehaviour
    {
        private PlayerController _player;
        [SerializeField] private GameObject child;
        [SerializeField] private Animator childAnimator;
        [SerializeField] private Animator animator;

        private void Start()
        {
            _player = FindObjectOfType<PlayerController>();
        }

        /*public void OnEnable()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
            animator.enabled = false;
            child.SetActive(true);
            childAnimator.Play("ChickRun");
        }*/

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
            {
                SoundManager.Instance.PlayCarHit();
                /*onHit();*/
                _player.ReturnChicksFromChick(gameObject);
            }
        }

        /*private void onHit()
        {
            if (animator != null)
            {
                childAnimator.enabled = false;
                child.SetActive(false);
                animator.enabled = true;
                animator.SetTrigger("Death");
            }
        }*/
    }
}