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

    [Header("Audio")]
    public AudioSource calibrationAudioSource;
    public AudioClip calibrationTone;

    [Header("dB-Konfiguration")]
    public float minDb = -60f;           // Leiseste Einstellung
    public float maxDb = 0f;             // Lauteste Einstellung
    public float startDb = 0f;           // Slider startet auf 0 dB (laut)

    private bool isPlaying = false;

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

        if (playButton != null) playButton.onClick.AddListener(TogglePlayPause);
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

    void TogglePlayPause()
    {
        if (calibrationAudioSource == null) return;

        if (isPlaying)
        {
            calibrationAudioSource.Pause();
            isPlaying = false;
        }
        else
        {
            calibrationAudioSource.Play();
            isPlaying = true;
        }
    }

    void OnConfirm()
    {
        if (calibrationAudioSource != null)
        {
            calibrationAudioSource.Stop();
        }

        // Schwelle als dB speichern
        GameManager.Instance.SetCalibratedThresholdDb(dbSlider.value);

        if (calibrationPanel != null) calibrationPanel.SetActive(false);
        GameManager.Instance.StartGame();
    }

    // Umrechnung dB → lineares Gain
    public static float DbToGain(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }

    // Umrechnung lineares Gain → dB (falls mal gebraucht)
    public static float GainToDb(float gain)
    {
        if (gain <= 0f) return -80f; // Stille als -80 dB behandeln
        return 20f * Mathf.Log10(gain);
    }
}