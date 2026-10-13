using UnityEngine;
using UnityEngine.UI;

/// <summary>Two live views. The inset camera renders to a bounded texture, never over the main camera.</summary>
[DefaultExecutionOrder(100)]
public class SnakeViewSwitcher : MonoBehaviour
{
    [Range(0.15f, 0.4f)] public float insetWidth = 0.27f;
    public Vector3 headOffset = new Vector3(0f, 0.15f, 0.7f);
    [Range(45f, 110f)] public float povFieldOfView = 80f;

    private Camera orbit, pov;
    private Transform head;
    private JoystickController input;
    private Canvas canvas;
    private RawImage preview;
    private RectTransform frame;
    private RenderTexture texture;
    private const float PovDurationSeconds = 5f;
    private float povReturnAt;
    private bool povIsMain;
    private Rect lastSafeArea;
    private int screenWidth, screenHeight;

    public void Initialize(Camera orbitCamera, Transform snakeHead, JoystickController controls)
    {
        if (orbit != null) return;
        orbit = orbitCamera; head = snakeHead; input = controls;
        var cameraObject = new GameObject("Snake POV Camera");
        pov = cameraObject.AddComponent<Camera>();
        pov.CopyFrom(orbit);
        pov.fieldOfView = povFieldOfView;
        pov.nearClipPlane = 0.03f;
        // This camera deliberately has no AudioListener: retain the existing single listener.
        var overlay = new GameObject("Snake Camera Preview", typeof(RectTransform), typeof(Canvas));
        canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var border = new GameObject("Preview Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(overlay.transform, false);
        frame = border.GetComponent<RectTransform>();
        frame.anchorMin = frame.anchorMax = frame.pivot = Vector2.zero;
        border.GetComponent<Image>().color = new Color(0.05f, 0.05f, 0.05f, 0.95f);
        border.GetComponent<Image>().raycastTarget = false;
        var picture = new GameObject("Live View", typeof(RectTransform), typeof(RawImage));
        picture.transform.SetParent(border.transform, false);
        preview = picture.GetComponent<RawImage>();
        preview.raycastTarget = false; // JoystickController owns tap versus drag recognition.
        var rect = picture.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(3f, 3f); rect.offsetMax = new Vector2(-3f, -3f);
        UpdateLayout();
        FollowHead();
        ApplyViews();
    }

    void LateUpdate()
    {
        if (orbit == null || pov == null || input == null) return;
        if (Screen.width != screenWidth || Screen.height != screenHeight || Screen.safeArea != lastSafeArea)
            UpdateLayout();
        bool playing = LevelManager.Instance == null || LevelManager.Instance.CurrentState == GameState.Playing;
        canvas.enabled = playing;
        input.HasMiniView = playing;
        // Use real time: a slow frame rate or changed time scale must not extend the POV visit.
        if (povIsMain && (!playing || Time.unscaledTime >= povReturnAt))
        {
            povIsMain = false;
            ApplyViews();
        }
        else if (playing && input.ViewSwapRequested)
        {
            povIsMain = !povIsMain;
            if (povIsMain) povReturnAt = Time.unscaledTime + PovDurationSeconds;
            ApplyViews();
        }
        // Keep the main camera visible after Game Over; disable only the hidden inset camera.
        orbit.enabled = !povIsMain || playing;
        pov.enabled = povIsMain || playing;
        FollowHead();
    }

    private void FollowHead()
    {
        if (head != null)
            pov.transform.SetPositionAndRotation(head.TransformPoint(headOffset), head.rotation);
    }

    private void UpdateLayout()
    {
        screenWidth = Screen.width; screenHeight = Screen.height; lastSafeArea = Screen.safeArea;
        float width = Mathf.Min(lastSafeArea.width * insetWidth, lastSafeArea.height * 0.5f);
        float height = width * 0.625f;
        const float margin = 12f;
        Rect area = new Rect(lastSafeArea.xMax - width - margin, lastSafeArea.yMax - height - margin, width, height);
        frame.anchoredPosition = area.position; frame.sizeDelta = area.size;
        input.MiniViewRect = area; input.HasMiniView = true;
        int texWidth = Mathf.Clamp(Mathf.RoundToInt(width), 128, 768);
        int texHeight = Mathf.Max(1, Mathf.RoundToInt(texWidth * (height - 6f) / (width - 6f)));
        orbit.targetTexture = null; pov.targetTexture = null;
        if (texture != null) { texture.Release(); Destroy(texture); }
        texture = new RenderTexture(texWidth, texHeight, 24) { name = "Snake inset view" };
        texture.Create();
        preview.texture = texture;
        ApplyViews();
    }

    private void ApplyViews()
    {
        Camera main = povIsMain ? pov : orbit;
        Camera mini = povIsMain ? orbit : pov;
        main.targetTexture = null;
        main.rect = new Rect(0f, 0f, 1f, 1f);
        main.aspect = (float)Mathf.Max(1, Screen.width) / Mathf.Max(1, Screen.height);
        mini.targetTexture = texture;
        mini.rect = new Rect(0f, 0f, 1f, 1f);
        mini.aspect = (float)texture.width / texture.height;
    }

    void OnDestroy()
    {
        if (input != null) input.HasMiniView = false;
        if (orbit != null) { orbit.targetTexture = null; orbit.enabled = true; orbit.ResetAspect(); }
        if (pov != null) { pov.targetTexture = null; Destroy(pov.gameObject); }
        if (canvas != null) Destroy(canvas.gameObject);
        if (texture != null) { texture.Release(); Destroy(texture); }
    }
}
