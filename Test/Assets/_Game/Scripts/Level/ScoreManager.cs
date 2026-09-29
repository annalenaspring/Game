using UnityEngine;
using TMPro;

/// <summary>
/// Verwaltet den Score und Highscore pro Level.
///
/// SETUP: Leeres GameObject in der Szene → dieses Script drauf.
///        Optional: scoreText zuweisen für Live-Anzeige im HUD.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI (optional)")]
    public TextMeshProUGUI scoreText;

    public int CurrentScore { get; private set; } = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// Punkte hinzufügen (z.B. beim Einsammeln eines Bountys).
    /// </summary>
    public void AddScore(int points)
    {
        CurrentScore += points;
        UpdateUI();
        Debug.Log($"Score: {CurrentScore}");
    }

    void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = CurrentScore.ToString();
    }

    /// <summary>
    /// Highscore für dieses Level speichern (via PlayerPrefs).
    /// levelId = eindeutiger Name z.B. "Level_01"
    /// </summary>
    public void SaveHighscore(string levelId)
    {
        int current = PlayerPrefs.GetInt("Highscore_" + levelId, 0);
        if (CurrentScore > current)
            PlayerPrefs.SetInt("Highscore_" + levelId, CurrentScore);
    }

    public int GetHighscore(string levelId)
    {
        return PlayerPrefs.GetInt("Highscore_" + levelId, 0);
    }
}
