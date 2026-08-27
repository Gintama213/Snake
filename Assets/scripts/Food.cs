using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private BoxCollider2D gridArea;

    private void Start()
    {
        RandomizePosition();
    }

    public void RandomizePosition()
    {
        Bounds bounds = this.gridArea.bounds;

        // Futter passend auf das 1er-Raster runden
        float x = Mathf.Round(Random.Range(bounds.min.x + 1, bounds.max.x - 1));
        float y = Mathf.Round(Random.Range(bounds.min.y + 1, bounds.max.y - 1));

        this.transform.position = new Vector3(x, y, 0.0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            RandomizePosition();
        }
    }
}