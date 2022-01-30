using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Audio;

public class MenuSystem : MonoBehaviour
{
    public TMP_Dropdown qualitySettings;
    public AudioMixer masterMixer;
    public TextMeshProUGUI currentMasterVolumePercentage;
    public Slider masterVolumeSlider;
    public TextMeshProUGUI currentMusicVolumePercentage;
    public Slider musicVolumeSlider;
    public TextMeshProUGUI currentSfxVolumePercentage;
    public Slider sfxVolumeSlider;
    public TextMeshProUGUI currentGammaPercentage;
    public Slider gammaSlider;

    private int qualityLevelBeforeChange;
    private float masterVolumeBeforeChange;
    private float musicVolumeBeforeChange;
    private float sfxVolumeBeforeChange;
    private float gammaValueBeforeChange;

    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        SetSlidersToCurrentValue();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("level");
    }

    public void BackToMenu()
    {
        ResetCurrentSettings();
    }

    private void SetSlidersToCurrentValue()
    {
        //Master Volume
        SetVolumePercentage(masterVolumeSlider.value, currentMasterVolumePercentage);

        //Music Volume
        SetVolumePercentage(musicVolumeSlider.value, currentMusicVolumePercentage);

        //SFX Volume
        SetVolumePercentage(sfxVolumeSlider.value, currentSfxVolumePercentage);

        //Gamma Value
        currentGammaPercentage.text = (gammaSlider.value * 100f).ToString("F0") + "%";
    }

    private void SetVolumePercentage (float volume, TextMeshProUGUI percentageText)
    {
        float volumePercentage = ((volume + 80f) * (5f / 4f));
        percentageText.text = volumePercentage.ToString("F0") + "%";
    }

    public void SetCurrentSettings()
    {
        qualityLevelBeforeChange = QualitySettings.GetQualityLevel();
        Debug.Log("Quality Set: " + qualityLevelBeforeChange);
        masterVolumeBeforeChange = masterVolumeSlider.value;
        musicVolumeBeforeChange = musicVolumeSlider.value;
        sfxVolumeBeforeChange = sfxVolumeSlider.value;
        gammaValueBeforeChange = gammaSlider.value;
    }

    public void ResetCurrentSettings()
    {
        //Quality Settings
        qualitySettings.value = qualityLevelBeforeChange;

        //Master Volume
        masterVolumeSlider.value = masterVolumeBeforeChange;
        masterMixer.SetFloat("masterVolume", (float)masterVolumeSlider.value);
        SetVolumePercentage(masterVolumeSlider.value, currentMasterVolumePercentage);

        //Music Volume
        musicVolumeSlider.value = musicVolumeBeforeChange;
        masterMixer.SetFloat("musicVolume", (float)musicVolumeSlider.value);
        SetVolumePercentage(musicVolumeSlider.value, currentMusicVolumePercentage);

        //SFX Volume
        sfxVolumeSlider.value = sfxVolumeBeforeChange;
        masterMixer.SetFloat("sfxVolume", (float)sfxVolumeSlider.value);
        SetVolumePercentage(sfxVolumeSlider.value, currentSfxVolumePercentage);

        //Gamma Value
        gammaSlider.value = gammaValueBeforeChange;
        RenderSettings.ambientLight = new Color(gammaSlider.value, gammaSlider.value, gammaSlider.value, 1f);
        currentGammaPercentage.text = (gammaSlider.value * 100f).ToString("F0") + "%";
    }

    public void SetMasterVolume()
    {
        //Master Volume
        masterMixer.SetFloat("masterVolume", (float)masterVolumeSlider.value);
        SetVolumePercentage(masterVolumeSlider.value, currentMasterVolumePercentage);
    }

    public void SetMusicVolume()
    {
        //Music Volume
        masterMixer.SetFloat("musicVolume", (float)musicVolumeSlider.value);
        SetVolumePercentage(musicVolumeSlider.value, currentMusicVolumePercentage);
    }

    public void SetSFXVolume()
    {
        //SFX Volume
        masterMixer.SetFloat("sfxVolume", (float)sfxVolumeSlider.value);
        SetVolumePercentage(sfxVolumeSlider.value, currentSfxVolumePercentage);
    }

    public void SetGamma()
    {
        //Gamma Value
        RenderSettings.ambientLight = new Color(gammaSlider.value, gammaSlider.value, gammaSlider.value, 1f);
        currentGammaPercentage.text = (gammaSlider.value * 100f).ToString("F0") + "%";
    }

    public void ApplySettings()
    {
        QualitySettings.SetQualityLevel(qualitySettings.value, true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
