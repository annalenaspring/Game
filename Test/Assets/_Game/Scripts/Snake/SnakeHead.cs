using UnityEngine;

/// <summary>
/// Steuert die Schlangenbewegung und Rotation.
/// Die Schlange bewegt sich permanent vorwärts.
/// Joystick X = Links/Rechts, Y = Hoch/Runter (relativ zur aktuellen Richtung).
/// Kein Rollen (Z-Rotation immer 0).
///
/// SETUP:
///   1. GameObject mit Sphere/Mesh als Schlangenkopf
///   2. Rigidbody hinzufügen: Gravity = false, Collision Detection = Continuous
///   3. SphereCollider: Is Trigger = true, Tag = "Player"
///   4. Dieses Script hinzufügen
///   5. JoystickController-Objekt in "joystick" Feld ziehen
///   6. SnakeTrail-Script auf gleiches GameObject (oder Kind-Objekt)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class SnakeHead : MonoBehaviour
{
    [Header("Bewegung")]
    [Tooltip("Vorwärtsgeschwindigkeit in Unity-Units pro Sekunde")]
    public float moveSpeed = 8f;

    [Tooltip("Maximale Drehgeschwindigkeit in Grad pro Sekunde")]
    public float maxTurnRate = 100f;

    [Header("Referenzen")]
    [Tooltip("Ziehe das JoystickController-Objekt hier rein")]
    public JoystickController joystick;

    // Interne Rotation (kein Roll – nur Pitch und Yaw)
    private float currentYaw;
    private float currentPitch;

    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Physik dreht nicht, wir drehen selbst

        // Startrotation merken
        currentYaw   = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
    }

    void FixedUpdate()
    {
        if (LevelManager.Instance != null &&
            LevelManager.Instance.CurrentState != GameState.Playing)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        ApplyRotation();
        MoveForward();
    }

    void ApplyRotation()
    {
        if (joystick == null) return;

        Vector2 input = joystick.GetInput();

        // Yaw = Links/Rechts um Welt-Y-Achse (kein Roll)
        // Pitch = Hoch/Runter um lokale X-Achse
        currentYaw   += input.x * maxTurnRate * Time.fixedDeltaTime;
        currentPitch -= input.y * maxTurnRate * Time.fixedDeltaTime;

        // Pitch clampen damit die Schlange nicht auf den Kopf dreht
        // Für "free 3D flight" diese Zeile auskommentieren
        currentPitch = Mathf.Clamp(currentPitch, -85f, 85f);

        // Rotation setzen – Z bleibt 0 → kein Roll
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
    }

    void MoveForward()
    {
        rb.linearVelocity = transform.forward * moveSpeed;
    }

    // Kollision mit Spur oder Levelobjekten → Game Over
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Trail") || other.CompareTag("LevelObject"))
        {
            LevelManager.Instance?.GameOver();
        }
    }
}
