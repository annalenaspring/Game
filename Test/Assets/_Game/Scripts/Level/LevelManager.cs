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

    private bool replayArmed;
    private bool restarting;
    private int replayFinger = -1;
    private Vector2 replayStart;
    private float replayStarted;
    private bool replayMoved;

    void Awake()
    {
        // Singleton-Pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (winPanel != null) winPanel.SetActive(false);
    }

    void Update()
    {
        if (CurrentState != GameState.GameOver || restarting) return;
        // A two-finger gesture is always camera input, even if neither finger moved much.
        // Require a complete release before accepting another replay tap.
        if (Input.touchCount > 1)
        {
            replayArmed = false;
            replayFinger = -1;
            return;
        }
        // Fingers that were steering at the moment of death must all be lifted first.
        if (!replayArmed)
        {
            if (Input.touchCount == 0 && !Input.GetMouseButton(0)) replayArmed = true;
            return;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButtonDown(0))
        {
            replayFinger = 0; replayStart = Input.mousePosition;
            replayStarted = Time.unscaledTime; replayMoved = false;
        }
        if (replayFinger == 0)
        {
            replayMoved |= Vector2.Distance(Input.mousePosition, replayStart) > 20f;
            if (Input.GetMouseButtonUp(0))
            {
                if (!replayMoved && Time.unscaledTime - replayStarted <= 0.5f) RestartLevel();
                replayFinger = -1;
            }
        }
#else
        bool found = false;
        foreach (Touch touch in Input.touches)
        {
            if (replayFinger == -1 && touch.phase == TouchPhase.Began)
            {
                replayFinger = touch.fingerId; replayStart = touch.position;
                replayStarted = Time.unscaledTime; replayMoved = false;
            }
            if (touch.fingerId != replayFinger) continue;
            found = true;
            replayMoved |= Vector2.Distance(touch.position, replayStart) > 20f;
            if (touch.phase == TouchPhase.Ended)
            {
                if (!replayMoved && Time.unscaledTime - replayStarted <= 0.5f) RestartLevel();
                replayFinger = -1;
            }
            else if (touch.phase == TouchPhase.Canceled) replayFinger = -1;
        }
        if (!found) replayFinger = -1;
#endif
    }

    void OnApplicationFocus(bool focused)
    {
        if (!focused) { replayArmed = false; replayFinger = -1; }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// Wird aufgerufen wenn der Spieler stirbt.
    /// (Selbstkollision, Levelobjekt, zu weit weg)
    /// </summary>
    public void GameOver()
    {
        if (CurrentState != GameState.Playing) return;

        CurrentState = GameState.GameOver;
        replayArmed = false;
        replayFinger = -1;
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
        if (restarting) return;
        restarting = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }
}
