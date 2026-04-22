using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RocketMovement : MonoBehaviour
{
    [Header("Bewegung")]
    public float laneChangeSpeed = 15f;
    public float returnSpeed = 5f;
    public float laneWidth = 2.5f;
    public float inputCooldown = 1.0f;

    [Header("Lane-Grenzen")]
    public int minLane = -1;
    public int maxLane = 1;

    [Header("Barrel Roll")]
    public float rollSpeed = 720f;
    private bool isRolling = false;
    private float rollAngleCovered = 0f;
    private float rollDirection = 1f;

    public bool inputEnabled = true;
    private int currentLaneIndex = 0;
    private float currentMoveSpeed;
    private float nextInputTime = 0f;

    public bool externalLock = false; // Sperre von außen
    void Start() { currentMoveSpeed = laneChangeSpeed; }

    void Update()
    {
        if (!GameManager.Instance.isGameRunning) return;
        CheckForSpam();
        if (inputEnabled && !externalLock)
        {
            HandleInput();
        }

        ApplyMovement();
        ApplyRotation();
    }

    void CheckForSpam()
    {
        var kb = Keyboard.current;
        if (kb == null) return;
        bool keyPressed = kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame ||
                          kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame;

        if (keyPressed)
        {
            if (!inputEnabled) GameManager.Instance.RecordSpam("Autopilot aktiv");
            else if (Time.time < nextInputTime) GameManager.Instance.RecordSpam("Cooldown aktiv");
        }
    }

    void HandleInput()
    {
        var kb = Keyboard.current;
        if (kb == null || Time.time < nextInputTime) return;

        int oldLane = currentLaneIndex;
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) currentLaneIndex = Mathf.Max(currentLaneIndex - 1, minLane);
        else if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) currentLaneIndex = Mathf.Min(currentLaneIndex + 1, maxLane);

        if (currentLaneIndex != oldLane)
        {
            nextInputTime = Time.time + inputCooldown;
            string laneName = currentLaneIndex == -1 ? "Links" : (currentLaneIndex == 1 ? "Rechts" : "Mitte");
            GameManager.Instance.RecordLaneChange(laneName);
            currentMoveSpeed = laneChangeSpeed;
            StartRoll(currentLaneIndex < oldLane ? 1f : -1f);
        }
    }

    void StartRoll(float dir) { isRolling = true; rollAngleCovered = 0f; rollDirection = dir; }

    void ApplyMovement()
    {
        float targetX = currentLaneIndex * laneWidth;
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * currentMoveSpeed);
        transform.position = pos;
    }

    void ApplyRotation()
    {
        if (!isRolling) return;
        float step = rollSpeed * Time.deltaTime;
        rollAngleCovered += step;
        if (rollAngleCovered >= 90f)
        {
            isRolling = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else
        {
            transform.Rotate(Vector3.up * rollDirection * step);
        }
    }

    public void ReturnToCenter()
    {
        StopAllCoroutines();
        StartCoroutine(ReturnToCenterCoroutine());
    }

    IEnumerator ReturnToCenterCoroutine()
    {
        inputEnabled = false;
        currentMoveSpeed = returnSpeed;
        int oldLane = currentLaneIndex;
        currentLaneIndex = 0;
        if (oldLane != 0) StartRoll(oldLane > 0 ? 1f : -1f);
        while (Mathf.Abs(transform.position.x) > 0.01f) yield return null;
        transform.position = new Vector3(0, transform.position.y, transform.position.z);
        inputEnabled = true;
        currentMoveSpeed = laneChangeSpeed;
        nextInputTime = Time.time;
    }
}