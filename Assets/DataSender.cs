using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class DataSender : MonoBehaviour
{
    [Header("Google Forms Config")]
    public string googleFormURL = "https://docs.google.com/forms/d/e/1FAIpQLSfqFHbcky-34W8eSg-765YJNys6K5D3Op7bU9Dcc5t8nPHLpQ/formResponse";

    [Header("Entry IDs")]
    public string entryID_VP = "entry.1073678383";
    public string entryID_Quote = "entry.967751817";
    public string entryID_RT = "entry.1822734482";
    public string entryID_Spam = "entry.351015912";
    public string entryID_Log = "entry.903630587";

    [Header("NEUE IDs für Seitenstatistik")]
    public string entryID_CountL = "entry.XXXXX"; // Hier ID für "Gesamt Links"
    public string entryID_CountR = "entry.XXXXX"; // Hier ID für "Gesamt Rechts"
    public string entryID_SuccL = "entry.XXXXX";  // Hier ID für "Erfolg Links"
    public string entryID_SuccR = "entry.XXXXX";  // Hier ID für "Erfolg Rechts"

    public void SendToGoogle(string id, float q, float rt, int s, string log, int cL, int cR, int sL, int sR)
    {
        StartCoroutine(PostData(id, q, rt, s, log, cL, cR, sL, sR));
    }

    IEnumerator PostData(string id, float q, float rt, int s, string log, int cL, int cR, int sL, int sR)
    {
        WWWForm form = new WWWForm();
        form.AddField(entryID_VP, id);
        form.AddField(entryID_Quote, q.ToString("F2") + "%");
        form.AddField(entryID_RT, rt.ToString("F0") + " ms");
        form.AddField(entryID_Spam, s.ToString());
        form.AddField(entryID_Log, log);

        // NEUE Felder hinzufügen
        form.AddField(entryID_CountL, cL.ToString());
        form.AddField(entryID_CountR, cR.ToString());
        form.AddField(entryID_SuccL, sL.ToString());
        form.AddField(entryID_SuccR, sR.ToString());

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