using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elemente")]
    public TextMeshProUGUI collectableCounterText;

    [Header("Mobile-Steuerungs-Buttons")]
    public GameObject leftButton;
    public GameObject rightButton;

    [Header("Fragebogen PostGame")]
    public string surveyBaseURL_DE = "https://docs.google.com/forms/d/e/1FAIpQLSdOfX7V98EZ79nO2UzFegDMpLYV2_YIMYhUbxedAaM4eKI-fg/viewform?usp=pp_url&entry.667255470=";
    public string surveyBaseURL_EN = "https://docs.google.com/forms/d/e/1FAIpQLSe_FoctZk2qj21Krr5uWMiM-yCivClKju1sb1J2ynUbO5R_tQ/viewform?usp=pp_url&entry.667255470=";

    [Header("Audio (Reize)")]
    public AudioSource audioSource;
    public AudioClip reizNachLinks;
    public AudioClip reizNachRechts;

    [Header("Status & Tracking")]
    public bool isGameRunning = false;
    private float startTime;
    private float lastSoundStartTime = -1f;
    private string uniqueVPID;
    public int totalSpamCount = 0;

    [Header("Gruppe (Zufallszuweisung)")]
    public string probandengruppe = "";  // "EG" oder "KG"

    [Header("Seiten-Statistik")]
    private int countLinks = 0;
    private int countRechts = 0;
    private int successLinks = 0;
    private int successRechts = 0;
    private int totalCollected = 0;

    [Header("Kalibrierung")]
    public static float calibratedThresholdDb = 0.0f; 

    [Header("Check-Phase Referenz")]
    public CheckPhaseManager checkPhaseManager;

    [Header("Check-Phase Ergebnisse")]
    private int cp_hits = 0;
    private int cp_misses = 0;
    private int cp_falseAlarms = 0;
    private int cp_correctRejections = 0;

    [Header("Audio (Game-Sounds)")]
    public AudioSource rocketEngineSource;
    public AudioClip rocketEngineClip;
    public AudioSource ambientMusicSource;
    public AudioClip ambientMusicClip;

    [Header("Game-Sound Pegel (dB über Schwelle)")]
    public float rocketEngineDbAboveThreshold = 15f;
    public float ambientMusicDbAboveThreshold = 5f;

    public class SpawnData
    {
        public int index;
        public float spawnTime;
        public string aktiverReiz;
        public bool leftCollected = false;
        public bool rightCollected = false;
    }

    public List<SpawnData> allSpawns = new List<SpawnData>();
    public List<string> eventLogs = new List<string>();
    public List<float> reactionTimes = new List<float>();

    private string aktuellerReizTyp = "";

    void Awake()
    {
        Instance = this;
        //random id
        uniqueVPID = "VP_" + Random.Range(1000, 9999).ToString();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        //zfallszuweisung 50/50 zur EG oder KG
        probandengruppe = (Random.Range(0, 2) == 0) ? "EG" : "KG";
        Debug.Log($"Proband {uniqueVPID} wurde Gruppe {probandengruppe} zugewiesen.");
    }

    public void StartGame()
    {
        Time.timeScale = 1;
        startTime = Time.time;
        isGameRunning = true;
        totalCollected = 0;
        UpdateCollectableCounterUI();
        StartGameSounds();
        if (leftButton != null) leftButton.SetActive(true);
        if (rightButton != null) rightButton.SetActive(true);
        LogEvent("SYSTEM", "Experiment gestartet");
        LogEvent("GRUPPE", $"Proband ist in Gruppe: {probandengruppe}");
    }

    public float GetVolumeAboveThreshold(float dbAbove)
    {
        float targetDb = calibratedThresholdDb + dbAbove;
        return CalibrationManager.DbToGain(targetDb);
    }
    public float GetRocketEngineVolume()
    {
        return GetVolumeAboveThreshold(15f);  
    }
    public float GetAmbientMusicVolume()
    {
        return GetVolumeAboveThreshold(5f);  
    }
    public void StartGameSounds()
    {
        if (rocketEngineSource != null && rocketEngineClip != null)
        {
            rocketEngineSource.clip = rocketEngineClip;
            rocketEngineSource.volume = GetVolumeAboveThreshold(rocketEngineDbAboveThreshold);
            rocketEngineSource.loop = true;
            rocketEngineSource.spatialBlend = 0f;
            rocketEngineSource.Play();
        }

        if (ambientMusicSource != null && ambientMusicClip != null)
        {
            ambientMusicSource.clip = ambientMusicClip;
            ambientMusicSource.volume = GetVolumeAboveThreshold(ambientMusicDbAboveThreshold);
            ambientMusicSource.loop = true;
            ambientMusicSource.spatialBlend = 0f;
            ambientMusicSource.Play();
        }

        LogEvent("AUDIO", "Game-Sounds gestartet");
    }

    public void StopGameSounds()
    {
        if (rocketEngineSource != null) rocketEngineSource.Stop();
        if (ambientMusicSource != null) ambientMusicSource.Stop();

        LogEvent("AUDIO", "Game-Sounds gestoppt");
    }
    public string GetTimestamp() => (Time.time - startTime).ToString("F3");

    public void LogEvent(string kat, string det, string rTime = "")
    {
        string csvLine = $"{GetTimestamp()};{kat};{det};{rTime}";
        eventLogs.Add(csvLine);
        Debug.Log(csvLine);
    }
    public void SetCalibratedThresholdDb(float dbValue)
    {
        calibratedThresholdDb = dbValue;
        LogEvent("KALIBRIERUNG", $"Schwelle gesetzt: {dbValue:F1} dB");
    }

    public float GetReizVolume()
    {
        // Reiz ist 10 dB unter der individuellen Schwelle
        float reizDb = calibratedThresholdDb - 10f;
        return CalibrationManager.DbToGain(reizDb);
    }

    public void LogCheckPhaseTrial(int trialNr, bool hasSignal, bool heard)
    {
        string signalStr = hasSignal ? "Signal" : "Silence";
        string responseStr = heard ? "Gehoert" : "NichtGehoert";
        LogEvent("CHECK_TRIAL", $"Trial {trialNr} | {signalStr} | {responseStr}");
    }
    public void StartCheckPhase()
    {
        if (checkPhaseManager != null)
        {
            checkPhaseManager.StartCheckPhase();
        }
    }

    public void SetCheckPhaseResults(int hits, int misses, int falseAlarms, int correctRejections)
    {
        cp_hits = hits;
        cp_misses = misses;
        cp_falseAlarms = falseAlarms;
        cp_correctRejections = correctRejections;
        LogEvent("CHECK_PHASE_END", $"H:{hits} M:{misses} FA:{falseAlarms} CR:{correctRejections}");
    }

    public void PlayRandomSpawnSound()
    {
        int choice = Random.Range(0, 2);
        aktuellerReizTyp = (choice == 0) ? "Links" : "Rechts";

        if (choice == 0) countLinks++; else countRechts++;

        if (probandengruppe == "EG") //in EG wird sound gespielt
        {
            AudioClip clip = (choice == 0) ? reizNachLinks : reizNachRechts;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
                lastSoundStartTime = Time.time - startTime;
                LogEvent("REIZ", $"Sound {aktuellerReizTyp}");
            }
        }
        else //ansionsten kein soudn
        {
            lastSoundStartTime = Time.time - startTime;
            LogEvent("REIZ_KG", $"Kein Sound (KG), virtueller Reiz {aktuellerReizTyp}");
        }
    }

    public void RecordLaneChange(string laneName)
    {
        float currentT = Time.time - startTime;
        string rTimeStr = "";

        if (lastSoundStartTime > 0)
        {
            float diffMs = (currentT - lastSoundStartTime) * 1000f;
            reactionTimes.Add(diffMs);
            rTimeStr = diffMs.ToString("F0");
            lastSoundStartTime = -1f;
        }
        LogEvent("MANÖVER", $"Wechsel auf {laneName}", rTimeStr);
    }

    public void RecordSpam(string grund)
    {
        totalSpamCount++;
        LogEvent("SPAM", grund);
    }

    public void RecordSpawn(int index)
    {
        allSpawns.Add(new SpawnData { index = index, spawnTime = (Time.time - startTime), aktiverReiz = aktuellerReizTyp });
        LogEvent("SPAWN", $"Paar {index}");
    }

    public void RecordCollection(int index, string side)
    {
        var data = allSpawns.Find(x => x.index == index);
        if (data != null)
        {
            if (side == "Links") data.leftCollected = true; else data.rightCollected = true;

            bool korrekt = (data.aktiverReiz == side);

            if (korrekt)
            {
                if (side == "Links") successLinks++; else successRechts++;
            }
            totalCollected++;
            UpdateCollectableCounterUI();
            string kongruenz = korrekt ? "FOLGT_REIZ" : "IGNORIERT_REIZ";
            LogEvent("COLLECT", $"{side} (Index {index})", kongruenz);
        }
    }
    private void UpdateCollectableCounterUI()
    {
        if (collectableCounterText != null)
        {
            collectableCounterText.text = $"{totalCollected}";
        }
    }
    public void OpenSurveyDE()
    {
        string finalURL = surveyBaseURL_DE + uniqueVPID;
        Application.OpenURL(finalURL);
        Debug.Log("Deutscher Fragebogen mit ID " + uniqueVPID + " geöffnet.");
    }

    public void OpenSurveyEN()
    {
        string finalURL = surveyBaseURL_EN + uniqueVPID;
        Application.OpenURL(finalURL);
        Debug.Log("Englischer Fragebogen mit ID " + uniqueVPID + " geöffnet.");
    }
    public void EndGame()
    {
        isGameRunning = false;
        StopGameSounds();
        if (leftButton != null) leftButton.SetActive(false);
        if (rightButton != null) rightButton.SetActive(false);
        LogEvent("SYSTEM", "Hauptspiel beendet");
        StartCheckPhase();
    }

    public void OnSubmitData()
    {
        float avgRT = reactionTimes.Count > 0 ? reactionTimes.Average() : 0;
        int followed = successLinks + successRechts;
        float success = (followed / 20f) * 100f;

        string csvFullLog = "Zeit;Kategorie;Details;Reaktionszeit_ms\n" + string.Join("\n", eventLogs);

        DataSender sender = GetComponent<DataSender>();
        if (sender != null)
        {
            sender.SendToGoogle(
                uniqueVPID,
                success,
                avgRT,
                totalSpamCount,
                csvFullLog,
                countLinks,
                countRechts,
                successLinks,
                successRechts,
                cp_hits,
                cp_misses,
                cp_falseAlarms,
                cp_correctRejections,
                calibratedThresholdDb,
                probandengruppe
            );
        }
        
    }

}