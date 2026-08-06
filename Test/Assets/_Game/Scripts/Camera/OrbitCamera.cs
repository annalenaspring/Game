using UnityEngine;

/// <summary>
/// Orbitalkamera – dreht sich um den Mittelpunkt der Komposition.
/// Mobile: 1 Finger = drehen, 2 Finger = Zoom (Pinch)
/// Editor: Linker Maustaste/Trackpad-Drag = drehen, Scrollrad/Pinch = Zoom
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
    public float startDistance = 20f;

    [Header("Grenzen")]
    public float minElevation = 5f;
    public float maxElevation = 85f;
    public float minDistance  = 5f;
    public float maxDistance  = 50f;

    [Header("Geschwindigkeit")]
    public float orbitSensitivity = 0.3f;
    public float zoomSensitivity  = 0.05f;

    // Intern
    private float azimuth;
    private float elevation;
    private float distance;

    // Touch-State
    private Vector2 lastSingleTouchPos;
    private float   lastPinchDistance;

    void Start()
    {
        azimuth   = startAzimuth;
        elevation = startElevation;
        distance  = startDistance;
        UpdateCameraPosition();
    }

    void Update()
    {
        #if UNITY_EDITOR
        HandleMouseInput();
        #else
        HandleTouchInput();
        #endif

        UpdateCameraPosition();
    }

    // ── Editor: Maus / Trackpad ───────────────────────────────
    private Vector3 lastMousePos;

    void HandleMouseInput()
    {
        // Linke Maustaste ODER Trackpad-Drag → drehen
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePos;
            azimuth   += delta.x * orbitSensitivity;
            elevation -= delta.y * orbitSensitivity;
            elevation  = Mathf.Clamp(elevation, minElevation, maxElevation);
            lastMousePos = Input.mousePosition;
        }

        // Scrollrad oder Trackpad-Pinch → Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.001f)
        {
            distance -= scroll * 10f;
            distance  = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    // ── Mobile: Touch ─────────────────────────────────────────
    void HandleTouchInput()
    {
        int touchCount = Input.touchCount;

        if (touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            // Nur Touches auf der rechten Hälfte (links = Joystick)
            if (t.position.x < Screen.width * 0.5f) return;

            if (t.phase == TouchPhase.Began)
            {
                lastSingleTouchPos = t.position;
            }
            else if (t.phase == TouchPhase.Moved)
            {
                Vector2 delta = t.position - lastSingleTouchPos;
                azimuth   += delta.x * orbitSensitivity;
                elevation -= delta.y * orbitSensitivity;
                elevation  = Mathf.Clamp(elevation, minElevation, maxElevation);
                lastSingleTouchPos = t.position;
            }
        }
        else if (touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            float pinchDist = Vector2.Distance(t0.position, t1.position);

            if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
            {
                lastPinchDistance = pinchDist;
            }
            else
            {
                float delta = pinchDist - lastPinchDistance;
                distance -= delta * zoomSensitivity;
                distance  = Mathf.Clamp(distance, minDistance, maxDistance);
                lastPinchDistance = pinchDist;
            }
        }
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
