using UnityEngine;
using UnityEngine.UI;

/// <summary>Permanent perspective POV over the full-screen overview.</summary>
[DefaultExecutionOrder(100)]
public class SnakeViewSwitcher : MonoBehaviour
{
    public Vector3 headOffset = new Vector3(0f, 0.15f, 0.7f);
    [Range(45f, 110f)] public float povFieldOfView = 80f;
    private Camera pov;
    private Transform head;
    private Canvas canvas;
    private RawImage preview;
    private RectTransform frame;
    private RenderTexture texture;
    private Rect lastArea;

    public void Initialize(Camera orbit, Transform snakeHead, JoystickController controls)
    {
        if (pov != null) return;
        head = snakeHead;
        pov = new GameObject("Snake POV Camera").AddComponent<Camera>();
        pov.CopyFrom(orbit);
        pov.orthographic = false;
        pov.fieldOfView = povFieldOfView;
        pov.nearClipPlane = 0.03f;
        pov.rect = new Rect(0, 0, 1, 1);
        pov.aspect = 1f;
        var overlay = new GameObject("Snake POV Overlay", typeof(RectTransform), typeof(Canvas));
        canvas = overlay.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;
        var picture = new GameObject("POV Joystick", typeof(RectTransform), typeof(SoftCircleRawImage));
        picture.transform.SetParent(overlay.transform, false);
        preview = picture.GetComponent<SoftCircleRawImage>();
        preview.raycastTarget = false;
        frame = picture.GetComponent<RectTransform>();
        frame.anchorMin = frame.anchorMax = frame.pivot = Vector2.zero;
        UpdateLayout();
        FollowHead();
    }

    void LateUpdate()
    {
        if (pov == null) return;
        if (PortraitGameLayout.PovRect != lastArea) UpdateLayout();
        bool playing = LevelManager.Instance == null || LevelManager.Instance.CurrentState == GameState.Playing;
        canvas.enabled = pov.enabled = playing;
        FollowHead();
    }

    private void FollowHead()
    {
        if (head != null) pov.transform.SetPositionAndRotation(head.TransformPoint(headOffset), head.rotation);
    }

    private void UpdateLayout()
    {
        lastArea = PortraitGameLayout.PovRect;
        frame.anchoredPosition = lastArea.position; frame.sizeDelta = lastArea.size;
        int size = Mathf.Clamp(Mathf.RoundToInt(lastArea.width), 128, 768);
        if (texture != null && texture.width == size) return;
        pov.targetTexture = null;
        if (texture != null) { texture.Release(); Destroy(texture); }
        texture = new RenderTexture(size, size, 24) { name = "Circular Snake POV" };
        texture.Create(); pov.targetTexture = texture; preview.texture = texture;
    }

    void OnDestroy()
    {
        if (pov != null) { pov.targetTexture = null; Destroy(pov.gameObject); }
        if (canvas != null) Destroy(canvas.gameObject);
        if (texture != null) { texture.Release(); Destroy(texture); }
    }
}
