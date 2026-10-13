using UnityEngine;

/// <summary>
/// Orbitalkamera – dreht sich um den Mittelpunkt der Komposition.
/// Mobile: Oberer Bildschirmbereich = drehen / Pinch-Zoom.
/// Editor: Maus ziehen = drehen, Scrollrad = Zoom.
///
/// SETUP:
///   1. Dieses Script auf die Hauptkamera
///   2. levelCenter: Position des Stillleben-Mittelpunkts setzen
///      (kann ein leeres GameObject sein, das du in der Mitte der Komposition platzierst)
/// </summary>
public class OrbitCamera : MonoBehaviour
{
    [Header("Orbit-Zentrum")]
    [Tooltip("Um diesen Punkt dreht sich die Kamera – leeres GameObject in Szenenmitte")]
    public Transform levelCenter;

    [Header("Startposition")]
    [Tooltip("Startwinkel horizontal (Grad)")]
    public float startAzimuth = 0f;
    [Tooltip("Startwinkel vertikal (Grad) – 0 = Horizont, 90 = von oben")]
    public float startElevation = 25f;
    [Tooltip("Startabstand zum Zentrum")]
    public float startDistance = 65f;

    [Header("Grenzen")]
    public float minElevation = 5f;
    public float maxElevation = 85f;
    public float minDistance = 5f;
    public float maxDistance = 80f;
    public float zoomSensitivity = 0.05f;

    [Header("Geschwindigkeit")]
    public float orbitSensitivity = 0.3f;
    [SerializeField] private JoystickController touchInput;

    // Intern
    private float azimuth;
    private float elevation;
    private float distance;

    void Awake()
    {
        if (touchInput == null)
            touchInput = FindAnyObjectByType<JoystickController>();
    }

    void Start()
    {
        azimuth   = startAzimuth;
        elevation = startElevation;
        distance  = startDistance;
        UpdateCameraPosition();
        var head = FindAnyObjectByType<SnakeHead>();
        var mainCamera = GetComponent<Camera>();
        if (head != null && mainCamera != null && touchInput != null)
        {
            var views = GetComponent<SnakeViewSwitcher>();
            if (views == null) views = gameObject.AddComponent<SnakeViewSwitcher>();
            views.Initialize(mainCamera, head.transform, touchInput);
        }
    }

    void LateUpdate()
    {
        if (touchInput != null)
        {
            Vector2 delta = touchInput.CameraDelta;
            azimuth += delta.x * orbitSensitivity;
            elevation = Mathf.Clamp(elevation - delta.y * orbitSensitivity, minElevation, maxElevation);
            distance = Mathf.Clamp(distance - touchInput.ZoomDelta * zoomSensitivity, minDistance, maxDistance);
        }
        UpdateCameraPosition();
    }

    // ── Kameraposition berechnen ──────────────────────────────
    void UpdateCameraPosition()
    {
        Vector3 center = levelCenter != null ? levelCenter.position : Vector3.zero;

        // Kugelkoordinaten → kartesisch
        float azRad  = azimuth * Mathf.Deg2Rad;
        float elRad  = elevation * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            distance * Mathf.Cos(elRad) * Mathf.Sin(azRad),
            distance * Mathf.Sin(elRad),
            distance * Mathf.Cos(elRad) * Mathf.Cos(azRad)
        );

        transform.position = center + offset;
        transform.LookAt(center);
    }
}
