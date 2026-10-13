using UnityEngine;

/// <summary>Full-screen orthographic overview, orbiting the scene centre.</summary>
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    public Transform levelCenter;
    public float startAzimuth = 0f, startElevation = 25f, startDistance = 65f;
    public float minElevation = -85f, maxElevation = 85f;
    [Header("Orthografischer Bildausschnitt")]
    public float startHalfWidth = 24f;
    public float minHalfWidth = 5f, maxHalfWidth = 80f;
    public float orbitSensitivity = 0.3f, zoomSensitivity = 0.05f;
    [SerializeField] private JoystickController touchInput;
    private Camera overview;
    private float azimuth, elevation, halfWidth;

    void Start()
    {
        overview = GetComponent<Camera>();
        if (touchInput == null) touchInput = FindAnyObjectByType<JoystickController>();
        if (levelCenter == null) levelCenter = GameObject.Find("LevelCenter")?.transform;
        azimuth = startAzimuth; elevation = startElevation; halfWidth = startHalfWidth;
        overview.orthographic = true;
        overview.rect = new Rect(0, 0, 1, 1);
        ApplyPose();
        var head = FindAnyObjectByType<SnakeHead>();
        if (head != null && touchInput != null)
        {
            var views = GetComponent<SnakeViewSwitcher>();
            if (views == null) views = gameObject.AddComponent<SnakeViewSwitcher>();
            views.Initialize(overview, head.transform, touchInput);
        }
    }

    void LateUpdate()
    {
        if (touchInput != null)
        {
            azimuth = Mathf.Repeat(azimuth + touchInput.CameraDelta.x * orbitSensitivity, 360f);
            elevation -= touchInput.CameraDelta.y * orbitSensitivity;
            halfWidth = Mathf.Clamp(halfWidth - touchInput.ZoomDelta * zoomSensitivity, minHalfWidth, maxHalfWidth);
        }
        ApplyPose();
    }

    private void ApplyPose()
    {
        elevation = Mathf.Clamp(elevation, Mathf.Max(-85f, minElevation), Mathf.Min(85f, maxElevation));
        overview.aspect = (float)Mathf.Max(1, Screen.width) / Mathf.Max(1, Screen.height);
        overview.orthographicSize = halfWidth / overview.aspect;
        Quaternion rotation = Quaternion.Euler(elevation, azimuth + 180f, 0f);
        Vector3 centre = levelCenter != null ? levelCenter.position : Vector3.zero;
        Vector2 screenCentre = PortraitGameLayout.OverviewCenter;
        // Translate the framing without changing the logical orbit pivot.
        Vector3 framing = new Vector3((2f * screenCentre.x - 1f) * halfWidth,
            (2f * screenCentre.y - 1f) * overview.orthographicSize, 0f);
        transform.SetPositionAndRotation(centre - rotation * (Vector3.forward * startDistance + framing), rotation);
    }
}
