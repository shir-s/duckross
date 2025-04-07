// using System;
// using UnityEngine;
// using Managers;
//
// namespace Objects
// {
//     public class ChickController : MonoBehaviour
//     {
//         private PlayerController _player;
//         [SerializeField] private GameObject child;
//         [SerializeField] private Animator childAnimator;
//         [SerializeField] private Animator animator;
//
//         private void Start()
//         {
//             _player = FindObjectOfType<PlayerController>();
//         }
//
//         /*public void OnEnable()
//         {
//             if (animator == null)
//             {
//                 animator = GetComponent<Animator>();
//             }
//             animator.enabled = false;
//             child.SetActive(true);
//             childAnimator.Play("ChickRun");
//         }*/
//
//         private void OnTriggerEnter(Collider other)
//         {
//             if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
//             {
//                 SoundManager.Instance.PlayCarHit();
//                 /*onHit();*/
//                 _player.ReturnChicksFromChick(gameObject);
//             }
//         }
//
//         /*private void onHit()
//         {
//             if (animator != null)
//             {
//                 childAnimator.enabled = false;
//                 child.SetActive(false);
//                 animator.enabled = true;
//                 animator.SetTrigger("Death");
//             }
//         }*/
//     }
// }

//
// using System.Collections;
// using UnityEngine;
// using Managers;
//
// namespace Objects
// {
//     public class ChickController : MonoBehaviour
//     {
//         private PlayerController _player;
//
//         [SerializeField] private GameObject child;
//         [SerializeField] private Animator childAnimator;
//         [SerializeField] private Animator animator;
//
//         private void Start()
//         {
//             _player = FindObjectOfType<PlayerController>();
//         }
//
//         private void OnTriggerEnter(Collider other)
//         {
//             if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
//             {
//                 SoundManager.Instance.PlayCarHit();
//
//                 // הפעלת אנימציית מוות על האפרוח
//                 if (childAnimator != null)
//                 {
//                     childAnimator.SetTrigger("IsDead");
//                     Debug.Log("Chick death animation triggered");
//                 }
//
//                 // הפעלת קורוטינה שמחזירה את האובייקט אחרי זמן קצר
//                 StartCoroutine(DelayedReturnToPool());
//             }
//         }
//
//         private IEnumerator DelayedReturnToPool()
//         {
//             // ממתין כדי לאפשר לאנימציית המוות להתנגן
//             yield return new WaitForSeconds(1.0f);
//
//             if (child != null)
//             {
//                 child.SetActive(false); // אפשר גם לוותר על זה אם האנימציה מספיקה
//             }
//
//             _player.ReturnChicksFromChick(gameObject);
//         }
//
//         private void OnEnable()
//         {
//             // איפוס אנימציות כשאובייקט חוזר מה־Pool
//             if (animator != null)
//             {
//                 animator.Rebind();
//                 animator.Update(0f);
//                 animator.enabled = false;
//             }
//
//             if (child != null)
//             {
//                 child.SetActive(true);
//             }
//
//             if (childAnimator != null)
//             {
//                 childAnimator.enabled = true;
//                 childAnimator.Play("ChickRun");
//             }
//         }
//     }
// }



using System.Collections;
using UnityEngine;
using Managers;

namespace Objects
{
    public class ChickController : MonoBehaviour
    {
        private PlayerController _player;

        [SerializeField] private GameObject child;              // אפרוח צהוב
        [SerializeField] private Animator childAnimator;        // ריצה
        [SerializeField] private Animator animator;             // ענן מוות

        private void Start()
        {
            _player = FindObjectOfType<PlayerController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Obstacle1") || other.CompareTag("Obstacle2"))
            {
                SoundManager.Instance.PlayCarHit();

                // מפסיק את האנימציה הרגילה של האפרוח
                if (childAnimator != null)
                {
                    childAnimator.enabled = false;
                }

                if (child != null)
                {
                    child.SetActive(false); // מסתיר את הצורה של האפרוח
                }

                // מפעיל את האנימציה של הענן (על האנימטור של האובייקט הראשי "Chick")
                if (animator != null)
                {
                    animator.enabled = true;
                    animator.SetTrigger("Death");
                }

                // מחכה רגע כדי לאפשר לאנימציה להתנגן
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
            // איפוס Animator של הענן
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
                animator.enabled = false;
            }

            // הפעלת האפרוח
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
