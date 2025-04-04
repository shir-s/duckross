using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Segments
{
    public class FinishSegment : MonoBehaviour
    {
        [SerializeField] private TMPro.TMP_Text chickToPassText;
        [SerializeField] private TMPro.TMP_Text chickCountText;

        
        private int minChicksToPass = 3;
        private int maxChicksToPass = 7;
        private int chicksToPass;

        public void Initialize(int chicksSpawned)
        {
            maxChicksToPass = (int)(chicksSpawned * 0.7f);
            // Randomly choose a number between minChicksToPass and maxChicksToPass (inclusive)
            chicksToPass = Random.Range(minChicksToPass, maxChicksToPass + 1);
            if(chickToPassText != null)
                chickToPassText.text = "Chicks to pass: " + chicksToPass.ToString();
        }
        
        private void OnEnable()
        {

        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                // Get the player's current chick count (assumes a static property in PlayerController)
                int currentChickCount = PlayerController.ChickCount;
                UpdateChickCountDisplay(currentChickCount);
                if (currentChickCount >= chicksToPass)
                {
                    EventManager.Instance.TriggerPlayerPassedFinishSegment();
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