using UnityEngine;

/// <summary>
/// Verwaltet den Spielzustand (Playing / GameOver / Won).
/// Singleton – einmal in der Szene platzieren, von überall erreichbar via LevelManager.Instance
///
/// SETUP: Leeres GameObject in der Szene erstellen → dieses Script drauf.
/// </summary>
public class LevelManager : MonoBehaviour
{
    // Singleton: überall im Code mit LevelManager.Instance erreichbar
    public static LevelManager Instance { get; private set; }

    public GameState CurrentState { get; private set; } = GameState.Playing;

    // TODO: Diese GameObjects im Inspector zuweisen
    [Header("UI Panels (später befüllen)")]
    public GameObject gameOverPanel;
    public GameObject winPanel;

    [Header("Referenzen")]
    public SnakeTrail snakeTrail;

    void Awake()
    {
        // Singleton-Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Wird aufgerufen wenn der Spieler stirbt.
    /// (Selbstkollision, Levelobjekt, zu weit weg)
    /// </summary>
    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        Debug.Log("GAME OVER");

        if (snakeTrail != null)
            snakeTrail.ClearTrail();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        // TODO: Sound abspielen, Score anzeigen etc.
    }

    /// <summary>
    /// Wird aufgerufen wenn das finale Bounty gegessen wurde.
    /// </summary>
    public void Win()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.Won;
        Debug.Log("WIN");

        if (winPanel != null)
            winPanel.SetActive(true);

        // TODO: Highscore speichern etc.
    }

    /// <summary>
    /// Level neu starten (z.B. beim Klick auf "Retry").
    /// </summary>
    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
