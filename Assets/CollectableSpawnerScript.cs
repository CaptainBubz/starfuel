using UnityEngine;
using System.Collections;

public class CollectableSpawnerScript : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Vector3 spawnPos1;
    public Vector3 spawnPos2;
    public float delaySeconds = 4.350f;
    public float forceReturnPreload = 2.0f;
    public float soundPreload = 0.350f;
    public float lockInputPreload = 1.0f; // Die neuen 1000ms Sperre

    void Start() { StartCoroutine(SpawnRoutine()); }

    IEnumerator SpawnRoutine()
    {
        while (!GameManager.Instance.isGameRunning) yield return null;
        RocketMovement player = FindFirstObjectByType<RocketMovement>();

        for (int i = 1; i <= 20; i++)
        {
            // 1. Phase: Warten bis zur Zentrierung (2s vor Spawn)
            float waitBeforeForce = delaySeconds - forceReturnPreload;
            if (waitBeforeForce > 0) yield return new WaitForSeconds(waitBeforeForce);

            if (player != null)
            {
                player.ReturnToCenter();
                Debug.Log($"[{GameManager.Instance.GetTimestamp()}] INFO: System-Zentrierung (2s vor Spawn {i})");
            }

            // 2. Phase: Warten bis zur Input-Sperre (1s vor Spawn)
            // Da wir schon 2s vor dem Spawn sind, warten wir 1s, um bei T-1s zu sein
            float waitBeforeLock = forceReturnPreload - lockInputPreload;
            yield return new WaitForSeconds(waitBeforeLock);

            if (player != null)
            {
                player.externalLock = true; // Sperrt den Input im Movement-Script
                Debug.Log($"[{GameManager.Instance.GetTimestamp()}] INFO: 1000ms Input-Sperre aktiv");
            }

            // 3. Phase: Warten bis zum Sound (350ms vor Spawn)
            float waitBeforeSound = lockInputPreload - soundPreload;
            yield return new WaitForSeconds(waitBeforeSound);

            GameManager.Instance.PlayRandomSpawnSound();

            // 4. Phase: Warten bis zum eigentlichen Spawn
            yield return new WaitForSeconds(soundPreload);

            // SPAWN
            GameManager.Instance.RecordSpawn(i);
            GameObject l = Instantiate(prefabToSpawn, spawnPos1, Quaternion.identity);
            l.GetComponent<CollecableScript>().Setup(i, "Links");

            GameObject r = Instantiate(prefabToSpawn, spawnPos2, Quaternion.identity);
            r.GetComponent<CollecableScript>().Setup(i, "Rechts");

            // 5. Phase: Sperre nach dem Spawn sofort wieder aufheben
            if (player != null)
            {
                player.externalLock = false;
                Debug.Log($"[{GameManager.Instance.GetTimestamp()}] INFO: Input wieder freigegeben");
            }
        }

        yield return new WaitForSeconds(5f);
        GameManager.Instance.EndGame();
    }
}