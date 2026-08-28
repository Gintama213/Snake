using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ST-10, ST-11, ST-12: Verwaltet Eingaben und Richtungswechsel der Schlange
/// </summary>
public class SnakeInputManager : MonoBehaviour
{
    /// <summary>
    /// Mögliche Bewegungsrichtungen
    /// </summary>
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    // Aktuelle und nächste Richtung
    private Direction currentDirection = Direction.Right;
    private Direction nextDirection = Direction.Right;

    /// <summary>
    /// ST-10: Eingaben von Spieler abfragen (WASD / Pfeiltasten)
    /// Wird jeden Frame aufgerufen
    /// </summary>
    public void HandleInput()
    {
        // WASD-Eingaben
        if (Input.GetKeyDown(KeyCode.W))
        {
            SetNextDirection(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            SetNextDirection(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            SetNextDirection(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            SetNextDirection(Direction.Right);
        }

        // Pfeiltasten als Alternative
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            SetNextDirection(Direction.Up);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            SetNextDirection(Direction.Down);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SetNextDirection(Direction.Left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SetNextDirection(Direction.Right);
        }
    }

    /// <summary>
    /// ST-11: Richtungswechsel umsetzen
    /// ST-12: Verhindert 180°-Turns (z.B. nicht von Right zu Left)
    /// </summary>
    /// <param name="newDirection">Die neue gewünschte Richtung</param>
    private void SetNextDirection(Direction newDirection)
    {
        // Verhindere 180°-Turns
        if (IsOppositeDirection(currentDirection, newDirection))
        {
            return; // Ignoriere den Input
        }

        nextDirection = newDirection;
    }

    /// <summary>
    /// Prüft, ob zwei Richtungen entgegengesetzt sind
    /// </summary>
    private bool IsOppositeDirection(Direction current, Direction input)
    {
        return (current == Direction.Up && input == Direction.Down) ||
               (current == Direction.Down && input == Direction.Up) ||
               (current == Direction.Left && input == Direction.Right) ||
               (current == Direction.Right && input == Direction.Left);
    }

    /// <summary>
    /// Wendet den nächsten Richtungswechsel an
    /// Wird vom GameManager vor der Bewegung aufgerufen
    /// </summary>
    public void ApplyDirectionChange()
    {
        currentDirection = nextDirection;
    }

    /// <summary>
    /// Gibt die aktuelle Bewegungsrichtung zurück
    /// </summary>
    public Direction GetCurrentDirection()
    {
        return currentDirection;
    }

    /// <summary>
    /// Gibt die aktuelle Richtung als Vector2 zurück
    /// (für Bewegungslogik)
    /// </summary>
    public Vector2Int GetDirectionVector()
    {
        return currentDirection switch
        {
            Direction.Up => Vector2Int.up,
            Direction.Down => Vector2Int.down,
            Direction.Left => Vector2Int.left,
            Direction.Right => Vector2Int.right,
            _ => Vector2Int.zero
        };
    }

    /// <summary>
    /// Setzt die Richtung zurück (z.B. für Neustart)
    /// </summary>
    public void ResetDirection()
    {
        currentDirection = Direction.Right;
        nextDirection = Direction.Right;
    }
}

/// <summary>
/// ST-13, ST-14: Verwaltet Futter-Spawning und Kollisionsprüfung
/// </summary>
public class FoodManager : MonoBehaviour
{
    // Referenz zum Grid-System (von ST-6)
    private GridSize gridSize;

    // Position des aktuellen Futters
    private Vector2Int foodPosition;

    // Speichert alle Schlangen-Segmente (als Set für schnelle Lookups)
    private HashSet<Vector2Int> snakePositions = new HashSet<Vector2Int>();

    /// <summary>
    /// Initialisierung mit Grid-Dimensionen
    /// </summary>
    public void Initialize(GridSize grid, List<Vector2Int> initialSnakeSegments)
    {
        gridSize = grid;

        // Schlangen-Positionen aktualisieren
        snakePositions.Clear();
        foreach (var segment in initialSnakeSegments)
        {
            snakePositions.Add(segment);
        }

        // Erstes Futter spawnen
        SpawnFood();
    }

    /// <summary>
    /// ST-13: Futter an zufälliger Position auf freier Gitterzelle spawnen
    /// </summary>
    public void SpawnFood()
    {
        Vector2Int newPosition;
        int maxAttempts = 100;
        int attempts = 0;

        // Versuche eine freie Zelle zu finden
        do
        {
            newPosition = new Vector2Int(
                Random.Range(0, gridSize.GridWidth),
                Random.Range(0, gridSize.GridHeight)
            );
            attempts++;
        }
        while (snakePositions.Contains(newPosition) && attempts < maxAttempts);

        if (attempts < maxAttempts)
        {
            foodPosition = newPosition;
        }
        else
        {
            Debug.LogWarning("Konnte keine freie Zelle für Futter finden!");
        }
    }

    /// <summary>
    /// Aktualisiert die Schlangen-Positionen (wird vom GameManager aufgerufen)
    /// </summary>
    public void UpdateSnakePositions(List<Vector2Int> segments)
    {
        snakePositions.Clear();
        foreach (var segment in segments)
        {
            snakePositions.Add(segment);
        }
    }

    /// <summary>
    /// ST-14: Prüft Kollision zwischen Schlangenkopf und Futter
    /// </summary>
    /// <param name="snakeHeadPosition">Position des Schlangenkopfs</param>
    /// <returns>true wenn Kopf Futter berührt, false sonst</returns>
    public bool CheckFoodCollision(Vector2Int snakeHeadPosition)
    {
        return snakeHeadPosition == foodPosition;
    }

    /// <summary>
    /// Gibt die aktuelle Futter-Position zurück
    /// (für Rendering in ST-15)
    /// </summary>
    public Vector2Int GetFoodPosition()
    {
        return foodPosition;
    }

    /// <summary>
    /// Wird aufgerufen wenn Futter gegessen wurde
    /// </summary>
    public void OnFoodEaten()
    {
        SpawnFood();
    }
}

/// <summary>
/// Hilfs-Klasse für Grid-Informationen (referenziert von Datenstruktur ST-6)
/// </summary>
public class GridSize
{
    public int GridWidth { get; private set; }
    public int GridHeight { get; private set; }

    public GridSize(int width, int height)
    {
        GridWidth = width;
        GridHeight = height;
    }
}