using UnityEngine;
using TMPro;

/// <summary>
/// Überwacht den Abstand der Schlange vom Levelzentrum.
/// Zu weit weg → Warnung → Countdown → Game Over.
///
/// SETUP:
///   1. Leeres GameObject in der Szene, dieses Script drauf
///   2. snakeHead: Schlangenkopf-Objekt zuweisen
///   3. levelCenter: Gleiche Transform wie in OrbitCamera
///   4. Optional: countdownText für UI-Anzeige
/// </summary>
public class BoundaryZone : MonoBehaviour
{
    [Header("Referenzen")]
    public Transform snakeHead;
    public Transform levelCenter;

    [Header("Abstände")]
    [Tooltip("Ab hier erscheint die Warnung")]
    public float warningRadius  = 30f;
    [Tooltip("Countdown-Dauer in Sekunden")]
    public float countdownTime  = 5f;

    [Header("UI (optional)")]
    [Tooltip("TextMeshPro-Element für den Countdown (kann leer bleiben)")]
    public TextMeshProUGUI countdownText;

    // Intern
    private float remainingTime;
    private bool  isCountingDown = false;

    void Update()
    {
        if (LevelManager.Instance == null ||
            LevelManager.Instance.CurrentState != GameState.Playing) return;

        if (snakeHead == null || levelCenter == null) return;

        float dist = Vector3.Distance(snakeHead.position, levelCenter.position);

        if (dist > warningRadius)
        {
            // Countdown starten oder weiterführen
            if (!isCountingDown)
            {
                isCountingDown  = true;
                remainingTime   = countdownTime;
            }

            remainingTime -= Time.deltaTime;

            // UI-Text aktualisieren
            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(true);
                countdownText.text = Mathf.Ceil(remainingTime).ToString("0");
            }

            if (remainingTime <= 0f)
            {
                LevelManager.Instance.GameOver();
            }
        }
        else
        {
            // Schlange ist zurück → Countdown stoppen
            isCountingDown = false;
            remainingTime  = countdownTime;

            if (countdownText != null)
                countdownText.gameObject.SetActive(false);
        }
    }

    // Grenzbereich in der Szene visualisieren (nur im Editor sichtbar)
    void OnDrawGizmosSelected()
    {
        if (levelCenter == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(levelCenter.position, warningRadius);
    }
}
