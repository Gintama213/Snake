using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ST-16 (Wandkollision) und ST-17 (Selbstkollision) – von MB.
/// Eigenes Skript: greift NICHT in die Snake.cs des Teams ein.
/// Es liest die Segment-Liste der Schlange nur aus und prüft die Kollisionen.
/// Läuft dank [DefaultExecutionOrder] immer NACH der Bewegung aus Snake.cs,
/// damit der Kopf schon bewegt ist, wenn wir prüfen.
/// </summary>
[DefaultExecutionOrder(100)]
public class Snakecollisionchecker : MonoBehaviour
{
    [Header("Referenzen (im Inspector zuweisen)")]
    [Tooltip("Das Snake-Objekt des Teams hier reinziehen")]
    [SerializeField] private Snake snake;
    [Tooltip("Das gleiche gridArea, das auch Food.cs benutzt")]
    [SerializeField] private BoxCollider2D gridArea;

    [Header("Test")]
    [Tooltip("Solange der Game-Over-Bildschirm (ST-21) fehlt: Szene bei Kollision neu laden")]
    [SerializeField] private bool reloadSceneOnCollision = true;

    /// <summary>Wird bei einer Kollision ausgelöst – Andockpunkt für ST-21.</summary>
    public System.Action OnGameOver;

    private FieldInfo _segmentsField;
    private bool _collided;

    private void Awake()
    {
        if (snake == null)
        {
            Debug.LogError("SnakeCollisionChecker: 'snake' ist nicht zugewiesen.");
            enabled = false;
            return;
        }

        // Nur-LESE-Zugriff auf die private Liste _segments aus Snake.cs,
        // damit die Team-Datei komplett unverändert bleibt.
        _segmentsField = typeof(Snake).GetField("_segments",
            BindingFlags.NonPublic | BindingFlags.Instance);

        if (_segmentsField == null)
            Debug.LogError("SnakeCollisionChecker: Feld '_segments' nicht gefunden – wurde es umbenannt?");
    }

    private void FixedUpdate()
    {
        if (_collided || _segmentsField == null) return;

        var segments = _segmentsField.GetValue(snake) as List<Transform>;
        if (segments == null || segments.Count == 0) return;

        Vector3 headPos = segments[0].position;               // Index 0 = Kopf
        Vector2Int headCell = Vector2Int.RoundToInt(headPos);

        if (HitsWall(headPos) || HitsSelf(headCell, segments))
        {
            _collided = true;
            OnGameOver?.Invoke();

            if (reloadSceneOnCollision)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    // ---- ST-16: Wandkollision (Spielfeldgrenzen) ----
    private bool HitsWall(Vector3 head)
    {
        if (gridArea == null) return false;

        Bounds b = gridArea.bounds;
        return head.x < b.min.x || head.x > b.max.x ||
               head.y < b.min.y || head.y > b.max.y;
    }

    // ---- ST-17: Selbstkollision (Kopf trifft Körpersegment) ----
    private bool HitsSelf(Vector2Int head, List<Transform> segments)
    {
        for (int i = 1; i < segments.Count; i++)   // ab 1, weil Index 0 der Kopf ist
        {
            if (Vector2Int.RoundToInt(segments[i].position) == head)
                return true;
        }
        return false;
    }
}