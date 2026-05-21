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
    public float lockInputPreload = 1.0f; 

    void Start() { StartCoroutine(SpawnRoutine()); }

    IEnumerator SpawnRoutine()
    {
        while (!GameManager.Instance.isGameRunning) yield return null;
        RocketMovement player = FindFirstObjectByType<RocketMovement>();

        for (int i = 1; i <= 20; i++)
        {

            float waitBeforeForce = delaySeconds - forceReturnPreload;
            if (waitBeforeForce > 0) yield return new WaitForSeconds(waitBeforeForce);

            if (player != null)
            {
                player.ReturnToCenter();
                Debug.Log($"[{GameManager.Instance.GetTimestamp()}] INFO: System-Zentrierung (2s vor Spawn {i})");
            }

            float waitBeforeLock = forceReturnPreload - lockInputPreload;
            yield return new WaitForSeconds(waitBeforeLock);

            if (player != null)
            {
                player.externalLock = true; 
                Debug.Log($"[{GameManager.Instance.GetTimestamp()}] INFO: 1000ms Input-Sperre aktiv");
            }

            float waitBeforeSound = lockInputPreload - soundPreload;
            yield return new WaitForSeconds(waitBeforeSound);

            GameManager.Instance.PlayRandomSpawnSound();

            yield return new WaitForSeconds(soundPreload);

            GameManager.Instance.RecordSpawn(i);
            GameObject l = Instantiate(prefabToSpawn, spawnPos1, Quaternion.identity);
            l.GetComponent<CollecableScript>().Setup(i, "Links");

            GameObject r = Instantiate(prefabToSpawn, spawnPos2, Quaternion.identity);
            r.GetComponent<CollecableScript>().Setup(i, "Rechts");

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