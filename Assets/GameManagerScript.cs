using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elemente")]
    public GameObject startPanel;
    public TextMeshProUGUI collectableCounterText;

    [Header("Fragebogen Einstellungen")]
    public string surveyBaseURL = "https://docs.google.com/forms/d/e/1FAIpQLSdOfX7V98EZ79nO2UzFegDMpLYV2_YIMYhUbxedAaM4eKI-fg/viewform?usp=pp_url&entry.667255470=";


    public TextMeshProUGUI timerText;

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

    [Header("Seiten-Statistik")]
    private int countLinks = 0;
    private int countRechts = 0;
    private int successLinks = 0;
    private int successRechts = 0;
    private int totalCollected = 0;

    [Header("Kalibrierung")]
    public static float calibratedThresholdDb = 0.0f;  // Der vom Probanden eingestellte Wert (0-1)

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
        // Erzeugt eine anonyme ID für diesen Durchgang
        uniqueVPID = "VP_" + Random.Range(1000, 9999).ToString();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        Time.timeScale = 1;
        startTime = Time.time;
        isGameRunning = true;
        totalCollected = 0;
        UpdateCollectableCounterUI();
        StartGameSounds();
        LogEvent("SYSTEM", "Experiment gestartet");
    }

    // Berechnet Lautstärke X dB ÜBER der Schwelle
    public float GetVolumeAboveThreshold(float dbAbove)
    {
        float targetDb = calibratedThresholdDb + dbAbove;
        return CalibrationManager.DbToGain(targetDb);
    }

    // Spezifische Methoden für die einzelnen Sounds:
    public float GetRocketEngineVolume()
    {
        return GetVolumeAboveThreshold(15f);  // 15 dB über Schwelle
    }

    public float GetAmbientMusicVolume()
    {
        return GetVolumeAboveThreshold(5f);   // 5 dB über Schwelle
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

    // Formatiert Log direkt für CSV (Zeit;Kategorie;Details;Reaktionszeit)
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

        // NEU: Reize zählen
        if (choice == 0) countLinks++; else countRechts++;

        AudioClip clip = (choice == 0) ? reizNachLinks : reizNachRechts;
        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            lastSoundStartTime = Time.time - startTime;
            LogEvent("REIZ", $"Sound {aktuellerReizTyp}");
        }
    }

    public void RecordLaneChange(string laneName)
    {
        float currentT = Time.time - startTime;
        string rTimeStr = "";

        // Reaktionszeit nur beim ersten Wechsel nach einem Sound messen
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

            // NEU: Erfolge pro Seite zählen
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
            collectableCounterText.text = $"Collected: {totalCollected}";
        }
    }
    public void OpenSurvey()
    {
        // Verknüpft den Basis-Link mit der aktuellen Probanden-ID
        string finalURL = surveyBaseURL + uniqueVPID;

        // Öffnet den Browser-Tab
        Application.OpenURL(finalURL);

        Debug.Log("Fragebogen mit ID " + uniqueVPID + " geöffnet.");
    }
    public void EndGame()
    {
        isGameRunning = false;
        StopGameSounds();
        LogEvent("SYSTEM", "Hauptspiel beendet");
        StartCheckPhase();
    }


    // DIESE FUNKTION AM EXPORT-BUTTON VERKNÜPFEN
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
                calibratedThresholdDb
            );
        }
        OpenSurvey();
    }

        void Update()
    {
        if (isGameRunning && timerText != null) timerText.text = GetTimestamp();
    }
}