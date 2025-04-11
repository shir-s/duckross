using System.Collections;
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

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
            {
                SoundManager.Instance.PlayCarHit();
                if (childAnimator != null)
                {
                    childAnimator.enabled = false;
                }

                if (child != null)
                {
                    child.SetActive(false);
                }
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.SetTrigger("Death");
                }
                StartCoroutine(DelayedReturnToPool());
            }
        }


        private IEnumerator DelayedReturnToPool()
        {
            yield return new WaitForSeconds(1.0f);
            _player.ReturnChicksFromChick(gameObject);
        }
        

        private void OnEnable()
        {
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.enabled = false;
            }
            if (child != null)
            {
                child.SetActive(true);
            }

            if (childAnimator != null)
            {
                childAnimator.enabled = true;
                childAnimator.Play("ChickRun");
            }
        }
    }
}
