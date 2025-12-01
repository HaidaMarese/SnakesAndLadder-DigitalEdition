using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Sound Effects")]
    [SerializeField] private AudioClip ladderSound;
    [SerializeField] private AudioClip snakeSound;
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip winSound;

    [Header("Audio Source")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Create AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.ignoreListenerPause = true;
    }

    public void PlayLadderSound()
    {
        if (ladderSound != null)
            audioSource.PlayOneShot(ladderSound);
    }

    public void PlaySnakeSound()
    {
        if (snakeSound != null)
            audioSource.PlayOneShot(snakeSound);
    }

    public void PlayMoveSound()
    {
        if (moveSound != null)
            audioSource.PlayOneShot(moveSound);
    }

    public void PlayWinSound()
    {
        if (winSound != null)
            audioSource.PlayOneShot(winSound);
    }
}