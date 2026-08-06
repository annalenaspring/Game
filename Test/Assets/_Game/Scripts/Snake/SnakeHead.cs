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

    // Interne Rotation (kein Roll – nur Pitch und Yaw)
    private float currentYaw;
    private float currentPitch;

    void Awake()
    {
        // Kinematisch: Physics bewegt das Objekt nicht, wir steuern selbst
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity  = false;

        currentYaw   = transform.eulerAngles.y;
        currentPitch = transform.eulerAngles.x;
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

        currentYaw   += input.x * maxTurnRate * Time.deltaTime;
        currentPitch -= input.y * maxTurnRate * Time.deltaTime;

        // Auskommentieren für vollständige 3D-Freiheit (auch über 90° Pitch)
        currentPitch = Mathf.Clamp(currentPitch, -85f, 85f);

        // Z = 0 → kein Roll
        transform.rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
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
