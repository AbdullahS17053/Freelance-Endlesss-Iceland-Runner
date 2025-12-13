using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Sources")]
    public AudioSource sourceMain;
    public AudioSource[] sourceEffects;

    [Header("UI Sliders")]
    public Slider mainVolumeSlider;
    public Slider effectsVolumeSlider;

    [Header("Audio Clips")]
    public AudioClip uiTheme;
    public AudioClip gameplayTheme;

    public AudioClip coinCollect;
    public AudioClip gemCollect;
    public AudioClip win;
    public AudioClip lost;
    public AudioClip revive;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Load saved values or default to 1
        mainVolumeSlider.value = PlayerPrefs.GetFloat("MainVolume", 1f);
        effectsVolumeSlider.value = PlayerPrefs.GetFloat("EffectsVolume", 1f);

        ChangeMainVolume(mainVolumeSlider.value);
        ChangeEffectsVolume(effectsVolumeSlider.value);

        // Listen to slider changes
        mainVolumeSlider.onValueChanged.AddListener(ChangeMainVolume);
        effectsVolumeSlider.onValueChanged.AddListener(ChangeEffectsVolume);
    }

    #region Music
    public void PlayGameplayTheme()
    {
        Play(sourceMain, gameplayTheme);
    }

    public void PlayUITheme()
    {
        Play(sourceMain, uiTheme);
    }
    #endregion

    #region Effects
    public void PlayCoinCollect() => PlayEffect(coinCollect);
    public void PlayGemCollect() => PlayEffect(gemCollect);
    public void PlayWin() => PlayEffect(win);
    public void PlayLost() => PlayEffect(lost);
    public void PlayRevive() => PlayEffect(revive);
    #endregion

    private void PlayEffect(AudioClip clip)
    {
        foreach (AudioSource source in sourceEffects)
        {
            if (!source.isPlaying)
            {
                Play(source, clip);
                return;
            }
        }
    }

    private void Play(AudioSource source, AudioClip clip)
    {
        source.clip = clip;
        source.Play();
    }

    // ================= SLIDER METHODS =================

    public void ChangeMainVolume(float value)
    {
        sourceMain.volume = value;
        PlayerPrefs.SetFloat("MainVolume", value);
    }

    public void ChangeEffectsVolume(float value)
    {
        foreach (AudioSource source in sourceEffects)
        {
            source.volume = value;
        }
        PlayerPrefs.SetFloat("EffectsVolume", value);
    }
}
