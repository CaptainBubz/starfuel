using UnityEngine;
using UnityEngine.UI;

public class IntroductionUIScript : MonoBehaviour
{
    [Header("UI")]
    public GameObject instructionPanel;
    public Button okButton;

    [Header("Referenz")]
    public CalibrationManager calibrationManager;

    void Start()
    {
        // Anleitungs-Panel ist initial NICHT aktiv (Kalibrierung kommt zuerst)
        if (instructionPanel != null) instructionPanel.SetActive(false);

        if (okButton != null)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(OnOkClicked);
        }
    }

    // Wird vom CalibrationManager aufgerufen, wenn Kalibrierung abgeschlossen
    public void ShowInstructionPanel()
    {
        if (instructionPanel != null) instructionPanel.SetActive(true);
    }

    void OnOkClicked()
    {
        // Anleitung verstecken
        if (instructionPanel != null) instructionPanel.SetActive(false);

        // Spiel direkt starten
        GameManager.Instance.StartGame();
    }
}