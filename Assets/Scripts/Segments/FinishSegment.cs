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

        private void OnEnable()
        {
            maxChicksToPass = InfiniteWorldGenerator.safeSegmentCount;
            // Randomly choose a number between minChicksToPass and maxChicksToPass (inclusive)
            chicksToPass = Random.Range(minChicksToPass, maxChicksToPass + 1);
            chicksToPass = 1;
            if(chickToPassText != null)
                chickToPassText.text = "Chicks to pass: " + chicksToPass.ToString();
        }

        private void OnTriggerEnter(Collider other)
        {
            if(other.CompareTag("Player"))
            {
                // Get the player's current chick count (assumes a static property in PlayerController)
                int currentChickCount = PlayerController.ChickCount;
                UpdateChickCountDisplay(currentChickCount);
                Debug.Log($"CHIKS INFO: {currentChickCount}, {chicksToPass}");
                if (currentChickCount >= chicksToPass)
                {
                    EventManager.Instance.TriggerPlayerPassedFinishSegment();
                }
            }
        }

        private void UpdateChickCountDisplay(int count)
        {
            if(chickCountText != null)
                Debug.Log("Updating chick count display");
                chickCountText.text = "Chicks number: " + count.ToString();
        }
    }
}