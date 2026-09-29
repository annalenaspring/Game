using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Verwaltet zwei Bounty-Typen:
///
/// PROGRESS BOUNTIES – sequenziell, eines nach dem anderen
///   Das letzte in der Liste = Final Bounty → isFinalBounty = true setzen
///
/// BONUS BOUNTIES – alle von Anfang an da, optional, geben nur Punkte
///
/// SETUP:
///   1. Leeres GameObject → dieses Script drauf
///   2. progressBountyPrefabs: Prefabs in Reihenfolge (letztes = Final)
///   3. progressSpawnPoints: Spawnpunkte für Progress Bounties
///   4. bonusBountyPrefabs: Optionale Bonus-Prefabs (anderer Look)
///   5. bonusSpawnPoints: Spawnpunkte für Bonus Bounties
/// </summary>
public class BountySpawner : MonoBehaviour
{
    public static BountySpawner Instance { get; private set; }

    [Header("Progress Bounties (sequenziell – Spielfortschritt)")]
    [Tooltip("Prefabs in Reihenfolge – letztes ist das Final Bounty")]
    public List<GameObject> progressBountyPrefabs = new List<GameObject>();

    [Header("Bonus Bounties (optional – alle sofort da)")]
    [Tooltip("Erscheinen alle gleichzeitig beim Start, anderer Look")]
    public List<GameObject> bonusBountyPrefabs = new List<GameObject>();

    [Header("Zufällige Spawn-Zone")]
    [Tooltip("Mittelpunkt der Zone in der Bounties erscheinen")]
    public Vector3 spawnCenter = Vector3.zero;
    [Tooltip("Radius der Zone")]
    public float spawnRadius = 10f;

    private int currentProgressIndex = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        SpawnNextProgress();
        SpawnAllBonus();
    }

    Vector3 RandomSpawnPosition()
    {
        Vector2 circle = Random.insideUnitCircle * spawnRadius;
        return spawnCenter + new Vector3(circle.x, Random.Range(-spawnRadius * 0.3f, spawnRadius * 0.3f), circle.y);
    }

    // ── Progress Bounties ─────────────────────────────────────
    void SpawnNextProgress()
    {
        if (currentProgressIndex >= progressBountyPrefabs.Count) return;
        if (progressBountyPrefabs[currentProgressIndex] == null) return;

        Instantiate(progressBountyPrefabs[currentProgressIndex], RandomSpawnPosition(), Quaternion.identity);
        currentProgressIndex++;
    }

    // ── Bonus Bounties ────────────────────────────────────────
    void SpawnAllBonus()
    {
        foreach (GameObject prefab in bonusBountyPrefabs)
        {
            if (prefab != null)
                Instantiate(prefab, RandomSpawnPosition(), Quaternion.identity);
        }
    }

    /// <summary>
    /// Wird von BountyBase aufgerufen wenn ein Bounty eingesammelt wurde.
    /// </summary>
    public void OnBountyCollected(BountyBase bounty)
    {
        // Nur bei Progress Bounties den nächsten spawnen
        if (!bounty.isFinalBounty && bounty.isProgressBounty)
            SpawnNextProgress();
    }

    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f);
        Gizmos.DrawWireSphere(spawnCenter, spawnRadius);
    }
}
