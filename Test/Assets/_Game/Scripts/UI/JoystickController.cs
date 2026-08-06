using UnityEngine;

/// <summary>
/// Virtueller Joystick – erkennt Touches auf der linken Bildschirmhälfte.
/// Kein UI-Element nötig, funktioniert rein über Touch-Koordinaten.
///
/// SETUP: Leeres GameObject in der Szene, dieses Script drauf.
///        In SnakeHead.cs das Feld "joystick" auf dieses Objekt ziehen.
/// </summary>
public class JoystickController : MonoBehaviour
{
    [Header("Einstellungen")]
    [Tooltip("Maximaler Radius des Joysticks in Pixeln")]
    public float maxRadius = 100f;

    [Tooltip("Toter Bereich: unter diesem Wert wird nichts gesendet (0–1)")]
    public float deadzone = 0.1f;

    // Interner State
    private int     activeTouchId   = -1;
    private Vector2 touchStartPos   = Vector2.zero;
    private Vector2 currentInput    = Vector2.zero;

    void Update()
    {
        // Im Unity-Editor: WASD-Steuerung zum Testen
        #if UNITY_EDITOR
        currentInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        return;
        #endif

        HandleTouch();
    }

    void HandleTouch()
    {
        foreach (Touch touch in Input.touches)
        {
            // Nur Touches auf der linken Bildschirmhälfte
            bool isLeftSide = touch.position.x < Screen.width * 0.5f;

            if (touch.phase == TouchPhase.Began && activeTouchId == -1 && isLeftSide)
            {
                activeTouchId = touch.fingerId;
                touchStartPos = touch.position;
            }

            if (touch.fingerId == activeTouchId)
            {
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                {
                    Vector2 delta = touch.position - touchStartPos;
                    Vector2 clamped = Vector2.ClampMagnitude(delta, maxRadius);
                    currentInput = clamped / maxRadius; // normalisiert auf -1 bis 1

                    // Toter Bereich
                    if (currentInput.magnitude < deadzone)
                        currentInput = Vector2.zero;
                }

                if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    activeTouchId = -1;
                    currentInput  = Vector2.zero;
                }
            }
        }

        // Kein aktiver Touch → Input zurücksetzen
        if (activeTouchId != -1)
        {
            bool stillActive = false;
            foreach (Touch t in Input.touches)
                if (t.fingerId == activeTouchId) { stillActive = true; break; }
            if (!stillActive) { activeTouchId = -1; currentInput = Vector2.zero; }
        }
    }

    /// <summary>
    /// Gibt den aktuellen Joystick-Input zurück.
    /// x = Links (-1) / Rechts (+1)
    /// y = Runter (-1) / Hoch (+1)
    /// Magnitude ist maximal 1.
    /// </summary>
    public Vector2 GetInput() => currentInput;
}
