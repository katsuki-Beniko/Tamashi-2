using UnityEngine;

public class Audio : MonoBehaviour
{
    private static Audio instance;
    private static AudioSource audioSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.playOnAwake = true;
            audioSource.loop = true;
        }

   }
}