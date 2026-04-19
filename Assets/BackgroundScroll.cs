using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Einstellungen")]
    public bool isFront;
    public float Scrollgeschwdivision = 10f;
    public float sideScrollMultiplier = 0.05f;

    [Header("Referenzen")]
    public Transform player;

    private Material mat;

    void Start()
    {
        mat = GetComponent<MeshRenderer>().material;
    }

    void Update()
    {
        float offsetY = Time.time / Scrollgeschwdivision;
        float offsetX = 0f;

        if (isFront && player != null)
        {
            offsetX = player.position.x * sideScrollMultiplier;
        }
        mat.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}