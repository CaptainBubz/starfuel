using UnityEngine;

public class CollecableScript : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 50f;

    [Header("Bewegung")]
    public float moveSpeed = 10f;

    [Header("Zähler (statisch, geteilt über alle Instanzen)")]
    public static int collectedCount = 0;

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            collectedCount++;
            Debug.Log("Collectables eingesammelt: " + collectedCount);
            Destroy(gameObject);
        }
    }
}