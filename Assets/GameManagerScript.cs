using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("UI Elemente")]
    public GameObject startPanel;
    public GameObject endPanel;
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
        LogEvent("SYSTEM", "Experiment gestartet");
    }

    public string GetTimestamp() => (Time.time - startTime).ToString("F3");

    // Formatiert Log direkt für CSV (Zeit;Kategorie;Details;Reaktionszeit)
    public void LogEvent(string kat, string det, string rTime = "")
    {
        string csvLine = $"{GetTimestamp()};{kat};{det};{rTime}";
        eventLogs.Add(csvLine);
        Debug.Log(csvLine);
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

            string kongruenz = korrekt ? "FOLGT_REIZ" : "IGNORIERT_REIZ";
            LogEvent("COLLECT", $"{side} (Index {index})", kongruenz);
        }
    }

    public void EndGame()
    {
        isGameRunning = false;
        Time.timeScale = 0;
        if (endPanel != null) endPanel.SetActive(true);
    }

    // DIESE FUNKTION AM EXPORT-BUTTON VERKNÜPFEN
    public void OnSubmitData()
    {
        float avgRT = reactionTimes.Count > 0 ? reactionTimes.Average() : 0;
        int followed = successLinks + successRechts;
        float success = (followed / 20f) * 100f;

        string csvFullLog = "Zeit;Kategorie;Details;Reaktionszeit_ms\n" + string.Join("\n", eventLogs);

        // NEU: Übergabe der vier neuen Werte an den DataSender
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
                successRechts
            );
        }
    }

    void Update()
    {
        if (isGameRunning && timerText != null) timerText.text = GetTimestamp();
    }
}