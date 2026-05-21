using UnityEngine;

public class FixedAspectRatio : MonoBehaviour
{
    [Header("Ziel-Aspect-Ratio")]
    public float targetAspectWidth = 16f;
    public float targetAspectHeight = 9f;

    private Camera cam;
    private float lastScreenWidth;
    private float lastScreenHeight;

    void Start()
    {
        cam = GetComponent<Camera>();
        ApplyAspectRatio();
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            ApplyAspectRatio();
        }
    }

    void ApplyAspectRatio()
    {
        float targetAspect = targetAspectWidth / targetAspectHeight;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1.0f)
        {
            // Rand obenuntem
            Rect rect = cam.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f;
            cam.rect = rect;
        }
        else
        {
            // rand rechtlinks
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = cam.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f;
            rect.y = 0;
            cam.rect = rect;
        }

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}