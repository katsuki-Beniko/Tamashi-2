using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource footstepSource; // Dedicated source for footsteps
    
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixerGroup sfxMixerGroup; // Connect to SFX mixer group
    
    [Header("Footstep Audio")]
    [SerializeField] private AudioClip footstepClip;
    [SerializeField] private float footstepVolumeMin = 0.8f;
    [SerializeField] private float footstepVolumeMax = 1.0f;
    [SerializeField] private float footstepPitchMin = 0.9f;
    [SerializeField] private float footstepPitchMax = 1.1f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create dedicated footstep audio source if it doesn't exist
            if (footstepSource == null)
            {
                footstepSource = gameObject.AddComponent<AudioSource>();
                footstepSource.playOnAwake = false;
                footstepSource.loop = true; // Set to loop for continuous footsteps
                footstepSource.volume = 1.0f; // Set to max since mixer will control final volume
            }
            
            // Connect footstep source to mixer
            SetupAudioMixerConnections();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void SetupAudioMixerConnections()
    {
        // If no mixer group is assigned, try to find it automatically
        if (sfxMixerGroup == null)
        {
            // Try to get the mixer group from the existing sfxSource
            if (sfxSource != null && sfxSource.outputAudioMixerGroup != null)
            {
                sfxMixerGroup = sfxSource.outputAudioMixerGroup;
            }
        }
        
        // Connect footstep source to the SFX mixer group
        if (footstepSource != null && sfxMixerGroup != null)
        {
            footstepSource.outputAudioMixerGroup = sfxMixerGroup;
            Debug.Log("Footstep audio connected to SFX mixer group");
        }
        else
        {
            Debug.LogWarning("Could not connect footstep audio to mixer group!");
        }
    }

    public void StartFootsteps()
    {
        if (footstepSource != null && footstepClip != null && !footstepSource.isPlaying)
        {
            // Set random variation
            footstepSource.pitch = Random.Range(footstepPitchMin, footstepPitchMax);
            footstepSource.volume = Random.Range(footstepVolumeMin, footstepVolumeMax);
            footstepSource.clip = footstepClip;
            footstepSource.Play();
            
            Debug.Log($"Started footstep audio at volume: {footstepSource.volume}");
        }
    }
    
    public void StopFootsteps()
    {
        if (footstepSource != null && footstepSource.isPlaying)
        {
            footstepSource.Stop();
            Debug.Log("Stopped footstep audio");
        }
    }
    
    public void PlayFootstep()
    {
        // For compatibility with old system - now just ensures footsteps are playing
        StartFootsteps();
    }
    
    public bool IsPlayingFootsteps()
    {
        return footstepSource != null && footstepSource.isPlaying;
    }
    
    // Method to manually set footstep volume (affects raw AudioSource volume)
    public void SetFootstepVolume(float volume)
    {
        footstepVolumeMin = Mathf.Clamp01(volume - 0.1f);
        footstepVolumeMax = Mathf.Clamp01(volume + 0.1f);
        
        if (footstepSource != null && footstepSource.isPlaying)
        {
            footstepSource.volume = volume;
        }
        
        Debug.Log($"Footstep volume set to: {volume}");
    }
    
    // Additional method for playing other SFX
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    // Method for playing SFX with custom volume
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }
}
