using UnityEngine;

/// <summary>
/// Positioniert ein Particle System als Richtungsindikator für den Joystick.
/// Erscheint nur wenn der POV-Bereich berührt wird.
///
/// SETUP:
///   1. Particle System aus dem Canvas rausnehmen → direkt in der Szene lassen
///   2. Dieses Script auf das Particle System Objekt
///   3. joystick-Feld auf den JoystickController ziehen
/// </summary>
public class JoystickVisual : MonoBehaviour
{
    [Header("Referenzen")]
    public JoystickController joystick;

    [Header("Einstellungen")]
    [Tooltip("Wie weit der Punkt vom Ursprung wegbewegt wird (in Pixeln auf dem Screen)")]
    public float visualRadius = 40f;

    [Tooltip("Wie schnell der Effekt ein- und ausblendet")]
    public float fadeSpeed = 8f;

    private ParticleSystem ps;
    private Camera mainCam;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        mainCam = Camera.main;

        // Beim Start: Partikel stoppen
        if (ps != null) ps.Stop();
    }

    void Start()
    {
        if (joystick == null)
            joystick = FindAnyObjectByType<JoystickController>();
    }

    void Update()
    {
        if (joystick == null || ps == null) return;

        bool active = joystick.IsJoystickActive;

        // Particle System ein/ausschalten
        if (active && !ps.isPlaying) ps.Play();
        if (!active && ps.isPlaying)  ps.Stop();

        if (!active) return;

        // Screen-Position berechnen: Ursprung + Input-Richtung
        Vector2 input      = joystick.GetInput();
        Vector2 screenPos  = joystick.JoystickOrigin + input * visualRadius;

        // Screen → World Position (auf einer festen Tiefe vor der Kamera)
        Vector3 worldPos = mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, 2f)); // 2 Unity Units vor Kamera

        transform.position = worldPos;
    }
}
