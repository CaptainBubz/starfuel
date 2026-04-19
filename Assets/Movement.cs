using UnityEngine;
using UnityEngine.InputSystem;

public class RocketMovement : MonoBehaviour
{
    [Header("Geschwindigkeit")]
    public float laneChangeSpeed = 15f;
    public float laneWidth = 2.5f;

    [Header("Cooldown Einstellungen")]
    public float laneChangeCooldown = 1.0f; // Zeit in Sekunden zwischen Wechseln
    private float nextLaneChangeTime = 0f;  // Zeitstempel, wann der nächste Wechsel erlaubt ist

    private int currentLaneIndex = 0;

    void Update()
    {
        HandleInput();
        ApplyMovement();
    }

    void HandleInput()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        // Prüfen, ob die aktuelle Zeit größer ist als die erlaubte Zeit für den nächsten Wechsel
        if (Time.time >= nextLaneChangeTime)
        {
            bool moved = false;

            if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame)
            {
                currentLaneIndex--;
                moved = true;
            }
            else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame)
            {
                currentLaneIndex++;
                moved = true;
            }

            // Wenn wir uns bewegt haben, setzen wir den neuen Zeitstempel
            if (moved)
            {
                nextLaneChangeTime = Time.time + laneChangeCooldown;
            }
        }
    }

    void ApplyMovement()
    {
        float targetX = currentLaneIndex * laneWidth;
        Vector3 newPos = transform.position;
        newPos.x = Mathf.Lerp(newPos.x, targetX, Time.deltaTime * laneChangeSpeed);
        transform.position = newPos;
    }
}