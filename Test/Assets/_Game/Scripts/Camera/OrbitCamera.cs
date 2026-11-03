using UnityEngine;

/// <summary>Full-screen perspective overview, orbiting the scene centre.</summary>
[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    public Transform levelCenter;
    public float startAzimuth = 0f, startElevation = 25f, startDistance = 65f;
    public float minElevation = -85f, maxElevation = 85f;
    [Header("Perspektivischer Bildausschnitt")]
    public float startHalfWidth = 24f;
    public float minHalfWidth = 5f, maxHalfWidth = 80f;
    public float orbitSensitivity = 0.3f, zoomSensitivity = 0.05f;
    [SerializeField] private JoystickController touchInput;
    public enum OverviewBackground { White, Gradient, BlurredHdri, OriginalHdri }
    [Header("Hintergrund nur in der Totale")]
    public OverviewBackground backgroundStyle = OverviewBackground.White;
    public Color gradientTop = new Color(0.70f, 0.76f, 0.82f);
    public Color gradientBottom = new Color(0.96f, 0.93f, 0.88f);
    [Range(0f, 1f)] public float backgroundBlur = 0.55f;
    private Skybox overviewSky;
    private Material softSkyMaterial;
    private Material gradientMaterial;
    private Material originalSkyMaterial;
    private bool addedSkybox;
    private bool originalSkyEnabled;
    private Camera overview;
    private float azimuth, elevation, halfWidth;

    void Start()
    {
        overview = GetComponent<Camera>();
        if (touchInput == null) touchInput = FindAnyObjectByType<JoystickController>();
        if (levelCenter == null) levelCenter = GameObject.Find("LevelCenter")?.transform;
        azimuth = startAzimuth; elevation = startElevation; halfWidth = startHalfWidth;
        overview.orthographic = false;
        overview.rect = new Rect(0, 0, 1, 1);
        ApplyPose();
        ConfigureBackground();
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
        UpdateBackground();
    }

    private void ConfigureBackground()
    {
        overviewSky = GetComponent<Skybox>();
        if (overviewSky != null)
        {
            originalSkyMaterial = overviewSky.material;
            originalSkyEnabled = overviewSky.enabled;
        }
        Shader shader = Resources.Load<Shader>("SoftPanorama");
        if (shader != null) softSkyMaterial = new Material(shader) { name = "Overview blurred HDRI" };
        Shader gradientShader = Resources.Load<Shader>("OverviewGradient");
        if (gradientShader != null) gradientMaterial = new Material(gradientShader) { name = "Overview gradient" };
        if (overviewSky == null)
        {
            overviewSky = gameObject.AddComponent<Skybox>();
            addedSkybox = true;
        }
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        if (overviewSky == null) return;
        if (backgroundStyle == OverviewBackground.White)
        {
            overviewSky.enabled = false;
            overview.clearFlags = CameraClearFlags.SolidColor;
            overview.backgroundColor = Color.white;
            return;
        }
        overview.clearFlags = CameraClearFlags.Skybox;
        if (backgroundStyle == OverviewBackground.Gradient && gradientMaterial != null)
        {
            gradientMaterial.SetColor("_Top", gradientTop);
            gradientMaterial.SetColor("_Bottom", gradientBottom);
            overviewSky.material = gradientMaterial;
            overviewSky.enabled = true;
            return;
        }
        Material source = originalSkyEnabled && originalSkyMaterial != null
            ? originalSkyMaterial : RenderSettings.skybox;
        bool useBlur = backgroundStyle == OverviewBackground.BlurredHdri && softSkyMaterial != null && source != null &&
            source.HasProperty("_MainTex") && source.GetTexture("_MainTex") is Texture2D;
        if (!useBlur)
        {
            overviewSky.material = originalSkyMaterial;
            overviewSky.enabled = !addedSkybox && originalSkyEnabled;
            return;
        }
        softSkyMaterial.SetTexture("_MainTex", source.GetTexture("_MainTex"));
        softSkyMaterial.SetColor("_Tint", source.HasProperty("_Tint") ? source.GetColor("_Tint") : Color.gray);
        softSkyMaterial.SetFloat("_Exposure", source.HasProperty("_Exposure") ? source.GetFloat("_Exposure") : 1f);
        softSkyMaterial.SetFloat("_Rotation", source.HasProperty("_Rotation") ? source.GetFloat("_Rotation") : 0f);
        softSkyMaterial.SetFloat("_Blur", backgroundBlur);
        overviewSky.material = softSkyMaterial;
        overviewSky.enabled = true;
        // The global skybox stays unchanged for the POV, ambient light and reflections.
        overview.clearFlags = CameraClearFlags.Skybox;
    }

    void OnDestroy()
    {
        if (overviewSky != null)
        {
            if (addedSkybox) Destroy(overviewSky);
            else { overviewSky.material = originalSkyMaterial; overviewSky.enabled = originalSkyEnabled; }
        }
        if (softSkyMaterial != null) Destroy(softSkyMaterial);
        if (gradientMaterial != null) Destroy(gradientMaterial);
    }

    private void ApplyPose()
    {
        elevation = Mathf.Clamp(elevation, Mathf.Max(-85f, minElevation), Mathf.Min(85f, maxElevation));
        overview.aspect = (float)Mathf.Max(1, Screen.width) / Mathf.Max(1, Screen.height);
        // Preserve the previous framing at the scene centre, with real perspective depth.
        float baseDistance = Mathf.Max(1f, startDistance);
        float baseWidth = Mathf.Max(0.1f, startHalfWidth);
        overview.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(baseWidth / (baseDistance * overview.aspect))
            * Mathf.Rad2Deg, 10f, 120f);
        // Pinch zoom moves the camera instead of changing the skybox's field of view.
        float distance = baseDistance * halfWidth / baseWidth;
        float halfHeight = distance * Mathf.Tan(overview.fieldOfView * 0.5f * Mathf.Deg2Rad);
        Quaternion rotation = Quaternion.Euler(elevation, azimuth + 180f, 0f);
        Vector3 centre = levelCenter != null ? levelCenter.position : Vector3.zero;
        Vector2 screenCentre = PortraitGameLayout.OverviewCenter;
        // Translate the framing without changing the logical orbit pivot.
        Vector3 framing = new Vector3((2f * screenCentre.x - 1f) * halfHeight * overview.aspect,
            (2f * screenCentre.y - 1f) * halfHeight, 0f);
        transform.SetPositionAndRotation(centre - rotation * (Vector3.forward * distance + framing), rotation);
    }
}
