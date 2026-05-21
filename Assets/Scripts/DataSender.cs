using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DataSender : MonoBehaviour
{
    [Header("Google Forms Config")]
    public string googleFormURL = "https://docs.google.com/forms/d/e/1FAIpQLSfqFHbcky-34W8eSg-765YJNys6K5D3Op7bU9Dcc5t8nPHLpQ/formResponse";

    [Header("Basis-Entry IDs")]
    public string entryID_VP = "entry.1073678383";
    public string entryID_Quote = "entry.967751817";
    public string entryID_RT = "entry.1822734482";
    public string entryID_Spam = "entry.351015912";
    public string entryID_Log = "entry.903630587";
   

    [Header("Seitenstatistik-Entry IDs")]
    public string entryID_CountL = "entry.XXXXX";
    public string entryID_CountR = "entry.XXXXX";
    public string entryID_SuccL = "entry.XXXXX";
    public string entryID_SuccR = "entry.XXXXX";

    [Header("Check-Phase-Entry IDs")]
    public string entryID_Hits = "entry.XXXXX";
    public string entryID_Misses = "entry.XXXXX";
    public string entryID_FalseAlarms = "entry.XXXXX";
    public string entryID_CorrectRejections = "entry.XXXXX";

    [Header("Kalibrierungs-Entry ID")]
    public string entryID_CalibThreshold = "entry.XXXXX";

    [Header("Probandengruppe-Entry ID")]
    public string entryID_Gruppe = "entry.XXXXX";

    public void SendToGoogle(
        string id, float q, float rt, int s, string log,
        int cL, int cR, int sL, int sR,
        int hits, int misses, int fa, int cr,
        float calibThreshold, string gruppe)
    {
        StartCoroutine(PostData(id, q, rt, s, log, cL, cR, sL, sR, hits, misses, fa, cr, calibThreshold, gruppe));
    }

    IEnumerator PostData(
    string id, float q, float rt, int s, string log,
    int cL, int cR, int sL, int sR,
    int hits, int misses, int fa, int cr,
    float calibThreshold, string gruppe)
    {
        WWWForm form = new WWWForm();

        // Basis-Daten
        form.AddField(entryID_VP, id);
        form.AddField(entryID_Quote, q.ToString("F2") + "%");
        form.AddField(entryID_RT, rt.ToString("F0") + " ms");
        form.AddField(entryID_Spam, s.ToString());
        form.AddField(entryID_Log, log);

        // Seitenstatistik
        form.AddField(entryID_CountL, cL.ToString());
        form.AddField(entryID_CountR, cR.ToString());
        form.AddField(entryID_SuccL, sL.ToString());
        form.AddField(entryID_SuccR, sR.ToString());

        // Check-Phase
        form.AddField(entryID_Hits, hits.ToString());
        form.AddField(entryID_Misses, misses.ToString());
        form.AddField(entryID_FalseAlarms, fa.ToString());
        form.AddField(entryID_CorrectRejections, cr.ToString());

        // Kalibrierung
        form.AddField(entryID_CalibThreshold, calibThreshold.ToString("F3"));

        // Probandengruppe
        form.AddField(entryID_Gruppe, gruppe);

        using (UnityWebRequest www = UnityWebRequest.Post(googleFormURL, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.Success)
                Debug.Log("DATEN ERFOLGREICH ÜBERMITTELT!");
            else
                Debug.LogError("FEHLER: " + www.error);
        }
    }
}