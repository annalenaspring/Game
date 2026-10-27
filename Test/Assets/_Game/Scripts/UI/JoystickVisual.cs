using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Zeigt einen leuchtenden Punkt als Richtungsindikator wenn der Spieler
/// den POV-Bereich berührt. Erscheint am Berührungspunkt, verschwindet beim Loslassen.
///
/// SETUP:
///   1. Im Canvas ein neues Image erstellen (Hierarchy → Canvas → UI → Image)
///   2. Image: Sprite = ein weiches Kreis-Sprite (oder Default UI Sprite)
///      Color = weiss oder cyan, Raycast Target = false
///   3. Dieses Script auf das Image-Objekt
///   4. joystick-Feld auf den JoystickController ziehen
///   5. dotImage-Feld auf dieses Image ziehen
///
/// TIPP für Glow: füge dem Image eine UI/Default Material mit Bloom hinzu,
/// oder einfach eine helle HDR-Farbe (funktioniert gut mit URP Bloom PostProcess).
/// </summary>
public class JoystickVisual : MonoBehaviour
{
    [Header("Referenzen")]
    public JoystickController joystick;
    public Image dotImage;

    [Header("Einstellungen")]
    [Tooltip("Wie weit der Punkt vom Ursprung wegbewegt wird (in Pixeln)")]
    public float visualRadius = 40f;

    [Tooltip("Wie schnell der Punkt ein- und ausblendet")]
    public float fadeSpeed = 8f;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas rootCanvas;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // CanvasGroup für Fade
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;

        rootCanvas = GetComponentInParent<Canvas>();
    }

    void Start()
    {
        if (joystick == null)
            joystick = FindAnyObjectByType<JoystickController>();
    }

    void Update()
    {
        bool active = joystick != null && joystick.IsJoystickActive;

        // Ein- / Ausblenden
        float targetAlpha = active ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);

        if (!active) return;

        // Punkt an Ursprung + Richtung positionieren
        Vector2 input = joystick.GetInput(); // -1 bis 1
        Vector2 originScreen = joystick.JoystickOrigin;

        // Dot-Position = Ursprung + Input-Richtung * Radius
        Vector2 dotScreenPos = originScreen + input * visualRadius;

        // Screen → RectTransform Position umrechnen
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.GetComponent<RectTransform>(),
            dotScreenPos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
            out Vector2 localPos))
        {
            rectTransform.localPosition = localPos;
        }
    }
}
