using UnityEngine;

/// <summary>
/// Basisklasse für alle Bounty-Typen.
/// Erbt von dieser Klasse für Score-, Ability- und Final-Bounties.
///
/// SETUP für ein Bounty-Prefab:
///   1. Sphere GameObject erstellen
///   2. SphereCollider → Is Trigger = true
///   3. Tag = "Bounty"
///   4. Dieses Script (oder eine Subklasse) drauf
///   5. Als Prefab in Assets/_Game/Prefabs/Bounties/ speichern
///   6. Material: leuchtendes/emissives Material zuweisen
/// </summary>
public class BountyBase : MonoBehaviour
{
    [Header("Bounty Einstellungen")]
    [Tooltip("Punkte die dieser Bounty gibt")]
    public int scoreValue = 100;

    [Tooltip("Gehört zum Spielfortschritt (sequenziell)")]
    public bool isProgressBounty = true;

    [Tooltip("Letzter Progress Bounty → schliesst das Level ab")]
    public bool isFinalBounty = false;

    // Damit der Bounty nicht mehrfach eingesammelt wird
    private bool collected = false;

    /// <summary>
    /// Wird aufgerufen wenn die Schlange diesen Bounty berührt.
    /// Kann in Subklassen überschrieben werden für Spezialeffekte.
    /// </summary>
    public virtual void Collect()
    {
        if (collected) return;
        collected = true;

        // Score hinzufügen
        ScoreManager.Instance?.AddScore(scoreValue);

        // BountySpawner informieren → nächsten Bounty spawnen
        BountySpawner.Instance?.OnBountyCollected(this);

        // Level abschliessen wenn Final-Bounty
        if (isFinalBounty)
            LevelManager.Instance?.Win();

        Destroy(gameObject);
    }

    // Optionale Animations-/Rotationslogik
    void Update()
    {
        // Bounty dreht sich langsam (visuelles Feedback)
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}
