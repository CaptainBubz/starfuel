using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CheckPhaseManager : MonoBehaviour
{
    [Header("UI")]
    public GameObject checkPhasePanel;
    public TextMeshProUGUI trialIndexText;
    public Button playButton;
    public Button heardButton;
    public Button notHeardButton;
    public Button submitButton;
    public TextMeshProUGUI submitInfoText;

    [Header("SurveySprachbutons")]
    public Button surveyDeButton;
    public Button surveyEnButton;
    public TextMeshProUGUI surveyChoiceText;

    [Header("Audio")]
    public AudioSource reizAudioSource;
    public AudioClip reizLinks;
    public AudioClip reizRechts;

    [Header("Konfiguration")]
    public int totalTrials = 6;
    public int signalTrials = 6;
    public float playDelaySeconds = 0.5f;

    // Zählvariablen
    private int hits = 0;
    private int misses = 0;
    private int falseAlarms = 0;
    private int correctRejections = 0;

    private int currentTrialIndex = 0;
    private List<bool> trialSequence;
    private bool isPlaying = false;
    private bool awaitingResponse = false;
    private bool hasSubmitted = false;

    public void StartCheckPhase()
    {
        // Trial-Sequenz vorbereiten
        trialSequence = new List<bool>();
        for (int i = 0; i < signalTrials; i++) trialSequence.Add(true);
        for (int i = 0; i < totalTrials - signalTrials; i++) trialSequence.Add(false);
        ShuffleList(trialSequence);

        // Zähler zurücksetzen
        currentTrialIndex = 0;
        hits = 0;
        misses = 0;
        falseAlarms = 0;
        correctRejections = 0;
        hasSubmitted = false;

        // UI Setup
        if (checkPhasePanel != null) checkPhasePanel.SetActive(true);

        // Alle UI-Elemente für die Trial-Phase einblenden
        if (playButton != null) playButton.gameObject.SetActive(true);
        if (heardButton != null) heardButton.gameObject.SetActive(true);
        if (notHeardButton != null) notHeardButton.gameObject.SetActive(true);
        if (trialIndexText != null) trialIndexText.gameObject.SetActive(true);

        // Submit-Button und Info erstmal verstecken
        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (submitInfoText != null) submitInfoText.gameObject.SetActive(false);
        // Sprach-Buttons auch erstmal verstecken
        if (surveyDeButton != null) surveyDeButton.gameObject.SetActive(false);
        if (surveyEnButton != null) surveyEnButton.gameObject.SetActive(false);
        if (surveyChoiceText != null) surveyChoiceText.gameObject.SetActive(false);

        // Button Listener (einmal registrieren, keine doppelten)
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(OnPlayClicked);
        }
        if (heardButton != null)
        {
            heardButton.onClick.RemoveAllListeners();
            heardButton.onClick.AddListener(() => OnResponseClicked(true));
        }
        if (notHeardButton != null)
        {
            notHeardButton.onClick.RemoveAllListeners();
            notHeardButton.onClick.AddListener(() => OnResponseClicked(false));
        }
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitClicked);
        }
        if (surveyDeButton != null)
        {
            surveyDeButton.onClick.RemoveAllListeners();
            surveyDeButton.onClick.AddListener(OnSurveyDeClicked);
        }
        if (surveyEnButton != null)
        {
            surveyEnButton.onClick.RemoveAllListeners();
            surveyEnButton.onClick.AddListener(OnSurveyEnClicked);
        }
        // AudioSource-Pegel an Kalibrierung anpassen
        if (reizAudioSource != null)
        {
            reizAudioSource.volume = GameManager.Instance.GetReizVolume();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        if (trialIndexText != null)
        {
            trialIndexText.text = $"Trial {currentTrialIndex + 1} / {totalTrials}";
        }

        if (playButton != null) playButton.interactable = !isPlaying && !awaitingResponse;
        if (heardButton != null) heardButton.interactable = awaitingResponse;
        if (notHeardButton != null) notHeardButton.interactable = awaitingResponse;
    }

    void OnPlayClicked()
    {
        if (isPlaying || awaitingResponse) return;
        StartCoroutine(PlayTrial());
    }

    IEnumerator PlayTrial()
    {
        isPlaying = true;
        UpdateUI();

        yield return new WaitForSeconds(playDelaySeconds);

        bool hasSignal = trialSequence[currentTrialIndex];

        if (hasSignal)
        {
            AudioClip clip = (Random.Range(0, 2) == 0) ? reizLinks : reizRechts;
            if (clip != null && reizAudioSource != null)
            {
                reizAudioSource.PlayOneShot(clip);
            }
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }

        isPlaying = false;
        awaitingResponse = true;
        UpdateUI();
    }

    void OnResponseClicked(bool heard)
    {
        if (!awaitingResponse) return;

        bool hasSignal = trialSequence[currentTrialIndex];

        if (hasSignal && heard) hits++;
        else if (hasSignal && !heard) misses++;
        else if (!hasSignal && heard) falseAlarms++;
        else if (!hasSignal && !heard) correctRejections++;

        GameManager.Instance.LogCheckPhaseTrial(currentTrialIndex + 1, hasSignal, heard);

        awaitingResponse = false;
        currentTrialIndex++;

        if (currentTrialIndex >= totalTrials)
        {
            EndCheckPhase();
        }
        else
        {
            UpdateUI();
        }
    }

    void EndCheckPhase()
    {
        // Ergebnisse speichern
        GameManager.Instance.SetCheckPhaseResults(hits, misses, falseAlarms, correctRejections);

        // Trial-UI verstecken
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (heardButton != null) heardButton.gameObject.SetActive(false);
        if (notHeardButton != null) notHeardButton.gameObject.SetActive(false);
        if (trialIndexText != null) trialIndexText.gameObject.SetActive(false);

        // Submit-Button und Info einblenden
        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;
        }
        if (submitInfoText != null)
        {
            submitInfoText.gameObject.SetActive(true);
            submitInfoText.text = "Studie beendet. Bitte klicke auf 'Daten senden', um die Studie abzuschließen.";
        }
    }

    void OnSubmitClicked()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;

        if (submitButton != null)
        {
            submitButton.interactable = false;
        }

        if (submitInfoText != null)
        {
            submitInfoText.text = "Daten werden gesendet...";
        }

        GameManager.Instance.OnSubmitData();

        StartCoroutine(ShowSurveyChoice());
    }

    IEnumerator ShowSurveyChoice()
    {
        // Kurz warten, bis der Datenupload Zeit hat zu starten
        yield return new WaitForSeconds(2f);

        // Submit-Button verstecken
        if (submitButton != null) submitButton.gameObject.SetActive(false);

        // Info-Text anpassen
        if (submitInfoText != null)
        {
            submitInfoText.text = "Vielen Dank! Bitte fülle noch den abschließenden Fragebogen aus.";
        }

        // Sprach-Auswahl anzeigen
        if (surveyChoiceText != null)
        {
            surveyChoiceText.gameObject.SetActive(true);
            surveyChoiceText.text = "Bitte wähle deine Sprache:\nPlease choose your language:";
        }

        if (surveyDeButton != null) surveyDeButton.gameObject.SetActive(true);
        if (surveyEnButton != null) surveyEnButton.gameObject.SetActive(true);
    }

    void OnSurveyDeClicked()
    {
        GameManager.Instance.OpenSurveyDE();
        ShowFinalThanks();
    }

    void OnSurveyEnClicked()
    {
        GameManager.Instance.OpenSurveyEN();
        ShowFinalThanks();
    }

    void ShowFinalThanks()
    {
        if (submitInfoText != null)
        {
            submitInfoText.text = "Der Fragebogen sollte sich in einem neuen Tab geöffnet haben.\n\nVielen Dank für deine Teilnahme!\nThank you for your participation!";
        }

        // Sprach-Buttons können auch versteckt werden
        if (surveyDeButton != null) surveyDeButton.gameObject.SetActive(false);
        if (surveyEnButton != null) surveyEnButton.gameObject.SetActive(false);
        if (surveyChoiceText != null) surveyChoiceText.gameObject.SetActive(false);
    }

    void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}