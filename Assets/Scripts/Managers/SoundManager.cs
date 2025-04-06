using UnityEngine;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip backgroundMusic;
        /*public AudioClip duckWalk;*/
        public AudioClip collectChick;
        public AudioClip carHit;
        /*public AudioClip buttonClick;*/
        public AudioClip gameOver;
        public AudioClip buttonClick;
        
        private void Awake()
        {
            // Singleton – ensures only one SoundManager exists
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            PlayMusic();
        }

        public void PlayMusic()
        {
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }
        
        // Optional helper methods
        /*public void PlayDuckWalk() => PlaySFX(duckWalk);*/
        public void PlayCollectChick() => PlaySFX(collectChick);
        public void PlayCarHit() => PlaySFX(carHit);
        /*public void PlayButtonClick() => PlaySFX(buttonClick);*/
        public void PlayGameOver() => PlaySFX(gameOver);
        public void PlayButtonClick() => PlaySFX(buttonClick);
    }
}
