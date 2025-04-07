using Managers;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Segments
{
    public class FinishSegment : MonoBehaviour
    {
        [SerializeField] private TMP_Text chickToPassText;
        [SerializeField] private TMP_Text chickCountText;
        [SerializeField] private float chickPassRatio = 0.5f;
        
        [SerializeField] private Sprite initialSprite; // Sprite for when the finish segment is initialized.
        [SerializeField] private Sprite passedSprite;  // Sprite for when the player passes the finish.
        
        private SpriteRenderer spriteRenderer;
        
        private int minChicksToPass = 3;
        private int maxChicksToPass = 7;
        private int chicksToPass;
        
        public void Initialize(int chicksSpawned)
        {
            maxChicksToPass = (int)(chicksSpawned * chickPassRatio)-1;
            maxChicksToPass = math.max(maxChicksToPass, minChicksToPass);
            // Randomly choose a number between minChicksToPass and maxChicksToPass (inclusive)
            chicksToPass = Random.Range(minChicksToPass, maxChicksToPass + 1);
            if(chickToPassText != null)
                /*chickToPassText.text = "Chicks to pass: " + chicksToPass.ToString();*/
                chickToPassText.text = chicksToPass.ToString();

            // Get or cache the SpriteRenderer and set it to the initial sprite.
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && initialSprite != null)
                spriteRenderer.sprite = initialSprite;
        }
        
        private void OnEnable()
        {
            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                // Get the player's current chick count (assumes a static property in PlayerController)
                int currentChickCount = PlayerController.ChickCount;
                Debug.Log(currentChickCount);
                UpdateChickCountDisplay(currentChickCount);
                if (currentChickCount >= chicksToPass)
                {
                    // Change sprite to passed sprite.
                    if (spriteRenderer != null && passedSprite != null)
                    {
                        spriteRenderer.sprite = passedSprite;
                    }
                    EventManager.Instance.TriggerPlayerPassedFinishSegment(chicksToPass);
                }
            }
        }
        
        private void UpdateChickCountDisplay(int count)
        {
            if(chickCountText != null)
                chickCountText.text = "Chicks number: " + count.ToString();
        }
    }
}