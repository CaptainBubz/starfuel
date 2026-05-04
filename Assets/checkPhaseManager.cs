using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class CheckPhaseManager : MonoBehaviour
{
    [Header("UI Container")]
    public GameObject checkPhasePanel;

    [Header("Trial-Phase UI")]
    public TextMeshProUGUI checkPhaseInfoText;     
    public TextMeshProUGUI trialIndexText;
    public Button playButton;
    public Button heardButton;
    public Button notHeardButton;

    [Header("Submit-Phase UI")]
    public Button submitButton;
    public TextMeshProUGUI submitInfoText;        

    [Header("Daten-Status UI")]
    public TextMeshProUGUI dataStatusInfoText;     

    [Header("Sprachauswahl UI")]
    public TextMeshProUGUI surveyInfoText;          
    public Button surveyDeButton;
    public Button surveyEnButton;

    [Header("Audio")]
    public AudioSource reizAudioSource;
    public AudioClip reizLinks;
    public AudioClip reizRechts;

    [Header("Konfiguration")]
    public int totalTrials = 6;
    public int signalTrials = 6;
    public float playDelaySeconds = 0.5f;

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
        trialSequence = new List<bool>();
        for (int i = 0; i < signalTrials; i++) trialSequence.Add(true);
        for (int i = 0; i < totalTrials - signalTrials; i++) trialSequence.Add(false);
        ShuffleList(trialSequence);

        currentTrialIndex = 0;
        hits = 0;
        misses = 0;
        falseAlarms = 0;
        correctRejections = 0;
        hasSubmitted = false;

        if (checkPhasePanel != null) checkPhasePanel.SetActive(true);

        if (checkPhaseInfoText != null) checkPhaseInfoText.gameObject.SetActive(true);
        if (trialIndexText != null) trialIndexText.gameObject.SetActive(true);
        if (playButton != null) playButton.gameObject.SetActive(true);
        if (heardButton != null) heardButton.gameObject.SetActive(true);
        if (notHeardButton != null) notHeardButton.gameObject.SetActive(true);

        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (submitInfoText != null) submitInfoText.gameObject.SetActive(false);

        if (dataStatusInfoText != null) dataStatusInfoText.gameObject.SetActive(false);

        if (surveyInfoText != null) surveyInfoText.gameObject.SetActive(false);
        if (surveyDeButton != null) surveyDeButton.gameObject.SetActive(false);
        if (surveyEnButton != null) surveyEnButton.gameObject.SetActive(false);

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
        GameManager.Instance.SetCheckPhaseResults(hits, misses, falseAlarms, correctRejections);

        if (checkPhaseInfoText != null) checkPhaseInfoText.gameObject.SetActive(false);
        if (trialIndexText != null) trialIndexText.gameObject.SetActive(false);
        if (playButton != null) playButton.gameObject.SetActive(false);
        if (heardButton != null) heardButton.gameObject.SetActive(false);
        if (notHeardButton != null) notHeardButton.gameObject.SetActive(false);

        if (submitButton != null)
        {
            submitButton.gameObject.SetActive(true);
            submitButton.interactable = true;
        }
        if (submitInfoText != null) submitInfoText.gameObject.SetActive(true);

    }

    void OnSubmitClicked()
    {
        if (hasSubmitted) return;
        hasSubmitted = true;

        if (submitButton != null) submitButton.gameObject.SetActive(false);
        if (submitInfoText != null) submitInfoText.gameObject.SetActive(false);

        if (dataStatusInfoText != null) dataStatusInfoText.gameObject.SetActive(true);

        GameManager.Instance.OnSubmitData();

        StartCoroutine(ShowSurveyChoice());
    }

    IEnumerator ShowSurveyChoice()
    {
        yield return new WaitForSeconds(2f);

        if (dataStatusInfoText != null) dataStatusInfoText.gameObject.SetActive(false);

        if (surveyInfoText != null) surveyInfoText.gameObject.SetActive(true);
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
        if (surveyInfoText != null) surveyInfoText.gameObject.SetActive(false);
        if (surveyDeButton != null) surveyDeButton.gameObject.SetActive(false);
        if (surveyEnButton != null) surveyEnButton.gameObject.SetActive(false);

        if (submitInfoText != null) submitInfoText.gameObject.SetActive(true);
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