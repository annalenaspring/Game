using UnityEngine;

/// <summary>POV circle: snake. Everywhere else: orbit/pinch. Roles are fixed at touch-down.</summary>
[DefaultExecutionOrder(-100)]
public class JoystickController : MonoBehaviour
{
    [Header("Steuerung")]
    [Min(1f)] public float maxRadius = 100f;
    [Range(0f, 1f)] public float deadzone = 0.1f;
    public Vector2 CameraDelta { get; private set; }
    public float ZoomDelta { get; private set; }

    private int joystickFinger = -1, cameraFinger = -1, zoomFinger = -1;
    private int previousCameraCount;
    private Vector2 origin, currentInput;
    private bool mouseOrbit, mouseSteer;
    private Vector2 lastMouse;

    void Update()
    {
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentState == GameState.Won)
        {
            ResetInput();
            return;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        ProcessMouse();
#else
        ProcessTouches(Input.touches);
#endif
        // The snake stays stopped; orbit and pinch remain available after death.
        if (LevelManager.Instance != null && LevelManager.Instance.CurrentState == GameState.GameOver)
        {
            currentInput = Vector2.zero;
        }
    }

    internal void ProcessTouches(Touch[] touches)
    {
        CameraDelta = Vector2.zero;
        ZoomDelta = 0f;

        if (!IsHeld(touches, joystickFinger)) { joystickFinger = -1; currentInput = Vector2.zero; }
        if (!IsHeld(touches, cameraFinger)) cameraFinger = -1;
        if (!IsHeld(touches, zoomFinger)) zoomFinger = -1;

        // Only Began assigns a role: crossing a boundary never changes it.
        foreach (Touch t in touches)
        {
            if (t.phase != TouchPhase.Began) continue;
            if (CanSteer && PortraitGameLayout.ContainsPov(t.position))
            {
                if (joystickFinger == -1) { joystickFinger = t.fingerId; origin = t.position; }
            }
            else if (cameraFinger == -1) cameraFinger = t.fingerId;
            else if (zoomFinger == -1) zoomFinger = t.fingerId;
        }

        Touch first = default, second = default;
        int cameraCount = 0;
        bool cameraBegan = false;
        foreach (Touch t in touches)
        {
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
            if (t.fingerId == joystickFinger)
            {
                currentInput = Vector2.ClampMagnitude((t.position - origin) / Mathf.Max(1f, maxRadius), 1f);
                if (currentInput.magnitude < deadzone) currentInput = Vector2.zero;
            }
            else if (t.fingerId == cameraFinger || t.fingerId == zoomFinger)
            {
                if (cameraCount++ == 0) first = t; else second = t;
                cameraBegan |= t.phase == TouchPhase.Began;
            }
        }
        // Rebase when switching between drag and pinch; no transition jumps.
        if (!cameraBegan && cameraCount == previousCameraCount)
        {
            if (cameraCount == 1) CameraDelta = first.deltaPosition;
            else if (cameraCount == 2)
                ZoomDelta = Vector2.Distance(first.position, second.position) -
                    Vector2.Distance(first.position - first.deltaPosition, second.position - second.deltaPosition);
        }
        previousCameraCount = cameraCount;
    }

    private bool CanSteer => LevelManager.Instance == null ||
        LevelManager.Instance.CurrentState == GameState.Playing;

    private void ProcessMouse()
    {
        CameraDelta = Vector2.zero; ZoomDelta = 0f; currentInput = Vector2.zero;
        Vector2 position = Input.mousePosition;
        bool overPov = CanSteer && PortraitGameLayout.ContainsPov(position);
        if (Input.GetMouseButtonDown(0))
        {
            mouseSteer = overPov; mouseOrbit = !overPov;
            origin = lastMouse = position;
        }
        if (Input.GetMouseButton(0))
        {
            if (mouseOrbit) CameraDelta = position - lastMouse;
            if (mouseSteer && CanSteer)
            {
                currentInput = Vector2.ClampMagnitude((position - origin) / Mathf.Max(1f, maxRadius), 1f);
                if (currentInput.magnitude < deadzone) currentInput = Vector2.zero;
            }
            lastMouse = position;
        }
        if (Input.GetMouseButtonUp(0)) mouseSteer = mouseOrbit = false;
        if (!overPov) ZoomDelta = Input.GetAxis("Mouse ScrollWheel") * 200f;
    }

    private static bool IsHeld(Touch[] touches, int id)
    {
        if (id < 0) return false;
        foreach (Touch t in touches)
            if (t.fingerId == id && t.phase != TouchPhase.Ended && t.phase != TouchPhase.Canceled) return true;
        return false;
    }

    private void ResetInput()
    {
        joystickFinger = cameraFinger = zoomFinger = -1;
        previousCameraCount = 0; mouseSteer = mouseOrbit = false;
        currentInput = CameraDelta = Vector2.zero; ZoomDelta = 0f;
    }
    void OnDisable() => ResetInput();
    void OnApplicationFocus(bool focused) { if (!focused) ResetInput(); }
    void OnApplicationPause(bool paused) { if (paused) ResetInput(); }
    public Vector2 GetInput() => currentInput;
}
