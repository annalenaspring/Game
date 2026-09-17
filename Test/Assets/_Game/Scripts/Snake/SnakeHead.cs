using UnityEngine;

/// <summary>
/// Steuert die Schlangenbewegung und Rotation.
/// Die Schlange bewegt sich permanent vorwärts.
/// Joystick X = Links/Rechts, Y = Hoch/Runter (relativ zur aktuellen Richtung).
/// Kein Rollen (Z-Rotation immer 0).
///
/// SETUP:
///   1. GameObject mit Sphere/Mesh als Schlangenkopf
///   2. Rigidbody hinzufügen: Is Kinematic = true, Use Gravity = false
///   3. SphereCollider: Is Trigger = true
///   4. Dieses Script hinzufügen
///   5. JoystickController-Objekt in "joystick" Feld ziehen
///   6. SnakeTrail-Script auf gleiches GameObject
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

    private Vector3 currentForward;

    void Awake()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;
    }

    void Start()
    {
        currentForward = transform.forward;
    }

    void Update()
    {
        if (LevelManager.Instance != null &&
            LevelManager.Instance.CurrentState != GameState.Playing)
            return;

        ApplyRotation();
        MoveForward();
    }

    void ApplyRotation()
    {
        if (joystick == null) return;

        Vector2 input = joystick.GetInput();
        float dt = Time.deltaTime;

        // Vollständig lokale Rotation → Steuerung immer relativ zur Flugrichtung
        transform.Rotate(Vector3.up,    input.x  * maxTurnRate * dt, Space.Self);
        transform.Rotate(Vector3.right, -input.y * maxTurnRate * dt, Space.Self);

        // Roll sanft korrigieren (kein harter Snap → keine Verwacklung)
        Vector3 fwd = transform.forward;
        Vector3 right = Vector3.Cross(Vector3.up, fwd).normalized;
        if (right.magnitude > 0.1f)
        {
            Quaternion zielRot = Quaternion.LookRotation(fwd, Vector3.Cross(fwd, right));
            transform.rotation = Quaternion.Slerp(transform.rotation, zielRot, 5f * dt);
        }
    }

    void MoveForward()
    {
        // transform.Translate: bewegt sich in lokaler Vorwärtsrichtung (hoch/runter inklusive)
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
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
