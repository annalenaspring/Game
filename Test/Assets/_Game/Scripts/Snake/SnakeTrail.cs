using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Erzeugt die permanente Spur der Schlange als Kette von Kettengliedern.
/// Jedes Glied ist ein Prefab (z.B. eine metallische Kugel/Kapsel).
///
/// SETUP:
///   1. Dieses Script auf das gleiche GameObject wie SnakeHead
///   2. Ein "ChainLink" Prefab erstellen:
///      - Sphere oder Capsule Mesh
///      - SphereCollider: Is Trigger = false
///      - Tag: "Trail"
///      - Material: metallisch (URP Lit, Metallic hoch)
///   3. Das Prefab in das "chainLinkPrefab" Feld ziehen
/// </summary>
public class SnakeTrail : MonoBehaviour
{
    [Header("Kettenglieder")]
    [Tooltip("Das Prefab für ein einzelnes Kettenglied")]
    public GameObject chainLinkPrefab;

    [Tooltip("Abstand in Unity-Units zwischen zwei Kettengliedern (kleiner = dichter)")]
    public float linkSpacing = 0.3f;

    [Tooltip("Die ersten N Glieder haben keine Kollision (Schonzeit am Anfang)")]
    public int gracePeriodLinks = 10;

    // Alle gespawnten Kettenglieder in der Reihenfolge ihrer Entstehung
    private List<GameObject> links = new List<GameObject>();

    // Wie weit sich der Kopf seit dem letzten Glied bewegt hat
    private float distanceSinceLastLink = 0f;
    private Vector3 lastPosition;

    void Start()
    {
        lastPosition = transform.position;
    }

    void Update()
    {
        if (LevelManager.Instance != null &&
            LevelManager.Instance.CurrentState != GameState.Playing)
            return;

        // Zurückgelegte Distanz berechnen
        float moved = Vector3.Distance(transform.position, lastPosition);
        distanceSinceLastLink += moved;
        lastPosition = transform.position;

        // Neues Glied spawnen wenn genug Abstand zurückgelegt
        while (distanceSinceLastLink >= linkSpacing)
        {
            SpawnLink();
            distanceSinceLastLink -= linkSpacing;
        }
    }

    void SpawnLink()
    {
        if (chainLinkPrefab == null) return;

        // Glied an aktueller Position spawnen
        GameObject link = Instantiate(chainLinkPrefab, transform.position, transform.rotation);
        link.tag = "Trail";

        // Kollision erst nach der Schonzeit aktivieren
        // Das neueste Glied bekommt zunächst keine Kollision
        Collider col = link.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;

        links.Add(link);

        // Das Glied das jetzt "alt genug" ist → Kollision einschalten
        int activateIndex = links.Count - 1 - gracePeriodLinks;
        if (activateIndex >= 0)
        {
            Collider oldCol = links[activateIndex].GetComponent<Collider>();
            if (oldCol != null)
                oldCol.enabled = true;
        }
    }

    /// <summary>
    /// Alle Kettenglieder löschen (z.B. als Power-Up).
    /// </summary>
    public void ClearTrail()
    {
        foreach (GameObject link in links)
            if (link != null) Destroy(link);
        links.Clear();
    }

    /// <summary>
    /// Anzahl der aktuellen Kettenglieder (für Score/UI).
    /// </summary>
    public int LinkCount => links.Count;
}
