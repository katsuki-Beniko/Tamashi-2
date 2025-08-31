using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSetting : MonoBehaviour
{
    [SerializeField] private AudioMixer auMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private float savedMusicVolume;
    private float savedSFXVolume;
    private void Start()
    {
        savedMusicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        savedSFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.value = savedMusicVolume;
        sfxSlider.value = savedSFXVolume;

        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);

        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    public void SetMusicVolume(float volume)
    {
        auMixer.SetFloat("Music", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetSFXVolume(float volume)
    {
        auMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}