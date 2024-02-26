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
    [SerializeField] TMP_Text animationText;
    [SerializeField] Toggle confirmationToggle;
    [SerializeField] Button quitButton;

    private void Awake()
    {
        instance = this;
        animationSlider.onValueChanged.AddListener(SetAnimationSpeed);
        confirmationToggle.onValueChanged.AddListener(delegate { SetConfirmationStatus(); });
    }

    private void Start()
    {
        if (!PlayerPrefs.HasKey("Confirm Choices")) //0 doesn't ask for confirmation, 1 does
            PlayerPrefs.SetInt("Confirm Choices", 1);
        confirmationToggle.isOn = (PlayerPrefs.GetInt("Confirm Choices") == 1);

        if (PlayerPrefs.HasKey("Animation Speed"))
            SetAnimationSpeed(PlayerPrefs.GetFloat("Animation Speed"));
        else
            SetAnimationSpeed(1f);
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
        animationText.text = value.ToString("F1");
        PlayerPrefs.SetFloat("Animation Speed", value);
    }
}
