using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// GameManager - Zentrale Verwaltung des Spiels
/// Orchestriert alle Systeme (Input, Bewegung, Futter, Kollision)
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("Einstellungen")]
    [SerializeField] private int gridWidth = 20;
    [SerializeField] private int gridHeight = 20;
    [SerializeField] private float moveSpeed = 0.2f; // Bewegung alle X Sekunden

    [Header("Manager")]
    private SnakeInputManager inputManager;
    private SnakeMovementManager movementManager; // Von ST-7 (Bewegungslogik)
    private FoodManager foodManager;

    // Für Timing der Schlangen-Bewegung
    private float moveTimer = 0f;

    private void Start()
    {
        // Manager initialisieren
        inputManager = gameObject.AddComponent<SnakeInputManager>();
        foodManager = gameObject.AddComponent<FoodManager>();
        movementManager = gameObject.AddComponent<SnakeMovementManager>();

        // Grid-Info erstellen
        GridSize grid = new GridSize(gridWidth, gridHeight);

        // Initiale Schlangen-Segmente (z.B. 3 Teile in der Mitte)
        List<Vector2Int> initialSegments = new List<Vector2Int>
        {
            new Vector2Int(gridWidth / 2, gridHeight / 2),       // Kopf
            new Vector2Int(gridWidth / 2 - 1, gridHeight / 2),   // Körper
            new Vector2Int(gridWidth / 2 - 2, gridHeight / 2)    // Schwanz
        };

        // Systeme initialisieren
        movementManager.Initialize(grid, initialSegments);
        foodManager.Initialize(grid, initialSegments);

        Debug.Log("Game initialisiert!");
    }

    private void Update()
    {
        // ST-10: Eingaben abfragen
        inputManager.HandleInput();

        // Timer für Bewegung
        moveTimer += Time.deltaTime;

        if (moveTimer >= moveSpeed)
        {
            moveTimer = 0f;

            // ST-11, ST-12: Richtungswechsel anwenden
            inputManager.ApplyDirectionChange();

            // ST-7: Schlange bewegen
            var moveDirection = inputManager.GetDirectionVector();
            movementManager.MoveSnake(moveDirection);

            // Schlangen-Segmente für Kollisionsprüfung
            var snakeSegments = movementManager.GetSnakeSegments();
            var snakeHead = snakeSegments[0];

            // ST-13, ST-14: Futter-Kollision prüfen
            foodManager.UpdateSnakePositions(snakeSegments);
            if (foodManager.CheckFoodCollision(snakeHead))
            {
                HandleFoodEaten();
            }
        }
    }

    /// <summary>
    /// Wird aufgerufen wenn Schlange Futter isst
    /// </summary>
    private void HandleFoodEaten()
    {
        // Schlange wächst (von ST-7)
        movementManager.GrowSnake();

        // Neues Futter spawnen
        foodManager.OnFoodEaten();

        Debug.Log("Futter gegessen!");
    }

    /// <summary>
    /// Gibt die aktuelle Schlangen-Position zurück (für Rendering)
    /// </summary>
    public List<Vector2Int> GetSnakeSegments()
    {
        return movementManager.GetSnakeSegments();
    }

    /// <summary>
    /// Gibt die Futter-Position zurück (für Rendering)
    /// </summary>
    public Vector2Int GetFoodPosition()
    {
        return foodManager.GetFoodPosition();
    }

    /// <summary>
    /// Prüft auf Kollision mit Wand oder sich selbst
    /// (ST-16, ST-17 - andere Tasks)
    /// </summary>
    public bool CheckGameOverCollision()
    {
        var snakeSegments = movementManager.GetSnakeSegments();
        var head = snakeSegments[0];

        // Wand-Kollision
        if (head.x < 0 || head.x >= gridWidth || head.y < 0 || head.y >= gridHeight)
        {
            return true;
        }

        // Selbst-Kollision (ab Index 4, da erste 3 Segmente nicht kollidieren)
        for (int i = 4; i < snakeSegments.Count; i++)
        {
            if (snakeSegments[i] == head)
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Platzhalter für SnakeMovementManager (von ST-7, ST-8)
/// Enthält die Bewegungslogik
/// </summary>
public class SnakeMovementManager : MonoBehaviour
{
    private List<Vector2Int> snakeSegments = new List<Vector2Int>();
    private GridSize gridSize;

    public void Initialize(GridSize grid, List<Vector2Int> initialSegments)
    {
        gridSize = grid;
        snakeSegments = new List<Vector2Int>(initialSegments);
    }

    public void MoveSnake(Vector2Int direction)
    {
        // Neue Position für den Kopf
        Vector2Int newHead = snakeSegments[0] + direction;
        
        // Kopf vorne hinzufügen
        snakeSegments.Insert(0, newHead);
        
        // Schwanz entfernen (außer bei Growth)
        snakeSegments.RemoveAt(snakeSegments.Count - 1);
    }

    public void GrowSnake()
    {
        // Letztes Segment wird nicht entfernt - Schlange wächst
        Vector2Int lastSegment = snakeSegments[snakeSegments.Count - 1];
        snakeSegments.Add(lastSegment);
    }

    public List<Vector2Int> GetSnakeSegments()
    {
        return new List<Vector2Int>(snakeSegments);
    }
}