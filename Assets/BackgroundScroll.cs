using UnityEngine;

public class BackgroundScroll : MonoBehaviour
{
    [Header("Einstellungen")]
    public bool isFront; // Wenn an, reagiert die Ebene auf Seitwärtsbewegung
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
        // 1. Topscroll (Y) - Läuft immer!
        float offsetY = Time.time / Scrollgeschwdivision;

        // 2. Sidescroll (X) - Nur wenn isFront aktiv ist und ein Player existiert
        float offsetX = 0f;

        if (isFront && player != null)
        {
            offsetX = player.position.x * sideScrollMultiplier;
        }

        // 3. Offset anwenden
        mat.mainTextureOffset = new Vector2(offsetX, offsetY);
    }
}