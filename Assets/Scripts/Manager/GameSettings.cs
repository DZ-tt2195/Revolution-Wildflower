using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MyBox;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public static GameSettings instance;
    [SerializeField] GameObject background;
    [SerializeField] Slider animationSlider;
    [SerializeField] Toggle confirmationToggle;
    [SerializeField] Toggle screenShakeToggle;
    [SerializeField] Button quitButton;

    private void Awake()
    {
        instance = this;
        animationSlider.onValueChanged.AddListener(SetAnimationSpeed);
        confirmationToggle.onValueChanged.AddListener(delegate { SetConfirmationStatus(); });
        screenShakeToggle.onValueChanged.AddListener(delegate { SetScreenShake(); });
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Confirm Choices")) //0 doesn't ask for confirmation, 1 does
            PlayerPrefs.SetInt("Confirm Choices", 1);
        confirmationToggle.isOn = (PlayerPrefs.GetInt("Confirm Choices") == 1);

        if (PlayerPrefs.HasKey("Animation Speed"))
            SetAnimationSpeed(PlayerPrefs.GetFloat("Animation Speed"));
        else
            SetAnimationSpeed(0.5f);

        if (!PlayerPrefs.HasKey("Screen Shake")) //0 doesn't screen shake, 1 does
            PlayerPrefs.SetInt("Screen Shake", 1);
        screenShakeToggle.isOn = (PlayerPrefs.GetInt("Screen Shake") == 1);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            SettingsScreen();
    }

    void SettingsScreen()
    {
        background.SetActive(!background.activeSelf);
        quitButton.gameObject.SetActive(SceneManager.GetActiveScene().name == "2. Level");
    }

    void SetConfirmationStatus()
    {
        PlayerPrefs.SetInt("Confirm Choices", confirmationToggle.isOn ? 1 : 0);
    }

    void SetAnimationSpeed(float value)
    {
        animationSlider.value = value;
        PlayerPrefs.SetFloat("Animation Speed", value);
    }

    void SetScreenShake()
    {
        PlayerPrefs.SetInt("Screen Shake", screenShakeToggle.isOn ? 1 : 0);
    }
}
