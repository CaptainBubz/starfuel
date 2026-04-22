using UnityEngine;

public class CollecableScript : MonoBehaviour
{
    public int spawnIndex;
    public string side;
    public float rotationSpeed = 50f;
    public float moveSpeed = 10f;

    public void Setup(int idx, string s) { spawnIndex = idx; side = s; }

    void Update()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime);
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        if (transform.position.y < -15f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.RecordCollection(spawnIndex, side);
            other.GetComponent<RocketMovement>().ReturnToCenter();
            Destroy(gameObject);
        }
    }
}