using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CalibrationManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject calibrationPanel;
    public Slider dbSlider;              // Jetzt in dB
    public TextMeshProUGUI dbValueText;  // Optional: zeigt aktuellen dB-Wert
    public Button playButton;
    public Button confirmButton;
    public TextMeshProUGUI soundActiveText;

    [Header("Audio")]
    public AudioSource calibrationAudioSource;
    public AudioClip calibrationTone;

    [Header("dB-Konfiguration")]
    public float minDb = -60f;           // Leiseste Einstellung
    public float maxDb = 0f;             // Lauteste Einstellung
    public float startDb = 0f;           // Slider startet auf 0 dB (laut)

    private bool isPlaying = false;

    [Header("Referenzen")]
    public IntroductionUIScript instructionManager;

    void Start()
    {
        // Slider-Konfiguration
        if (dbSlider != null)
        {
            dbSlider.minValue = minDb;
            dbSlider.maxValue = maxDb;
            dbSlider.value = startDb;
            dbSlider.onValueChanged.AddListener(OnDbSliderChanged);
        }

        if (playButton != null) playButton.onClick.AddListener(StartSound);
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirm);

        // AudioSource vorbereiten
        if (calibrationAudioSource != null)
        {
            calibrationAudioSource.clip = calibrationTone;
            calibrationAudioSource.loop = true;
            calibrationAudioSource.volume = DbToGain(startDb);
            calibrationAudioSource.spatialBlend = 0f;
            calibrationAudioSource.panStereo = 0f;
        }

        // Initial-Anzeige
        UpdateDbValueText(startDb);
        if (soundActiveText != null) soundActiveText.gameObject.SetActive(false);
        if (calibrationPanel != null) calibrationPanel.SetActive(true);
    }
    public void ShowCalibrationPanel()
    {
        if (calibrationPanel != null) calibrationPanel.SetActive(true);
    }
    void OnDbSliderChanged(float dbValue)
    {
        if (calibrationAudioSource != null)
        {
            calibrationAudioSource.volume = DbToGain(dbValue);
        }

        UpdateDbValueText(dbValue);
    }

    void UpdateDbValueText(float dbValue)
    {
        if (dbValueText != null)
        {
            dbValueText.text = $"{dbValue:F1} dB";
        }
    }

    void StartSound()
    {
        if (calibrationAudioSource == null) return;
        if (isPlaying) return;  // Schon am Spielen, nichts tun

        calibrationAudioSource.Play();
        isPlaying = true;

        // Sound-Active-Text einblenden
        if (soundActiveText != null)
        {
            soundActiveText.gameObject.SetActive(true);
            soundActiveText.text = "Der Sound spielt jetzt.";
        }

        // Play-Button deaktivieren, damit nicht nochmal geklickt wird
        if (playButton != null)
        {
            playButton.interactable = false;
        }
    }

    void OnConfirm()
    {
        if (calibrationAudioSource != null)
        {
            calibrationAudioSource.Stop();
        }
        isPlaying = false;
        GameManager.Instance.SetCalibratedThresholdDb(dbSlider.value);

        if (calibrationPanel != null) calibrationPanel.SetActive(false);

        // Spielanleitung anzeigen
        if (instructionManager != null) instructionManager.ShowInstructionPanel();
    }

    // Umrechnung dB → lineares Gain
    public static float DbToGain(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

}