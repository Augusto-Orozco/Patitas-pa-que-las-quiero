using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BendicionBehaviour : MonoBehaviour
{
    [SerializeField] private float shieldDuration = 4f;
    [SerializeField] private float minRespawnDelay = 5f;
    [SerializeField] private float maxRespawnDelay = 12f;
    [SerializeField] private int initialSpawnCount = 3;
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private float surfacePadding = 0.2f;
    [SerializeField] private Vector2 spawnAreaCenter = new Vector2(85f, 0f);
    [SerializeField] private Vector2 spawnAreaSize = new Vector2(270f, 50f);

    private Collider2D pickupCollider;
    private Renderer pickupRenderer;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        pickupRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        initialSpawnCount = Mathf.Max(1, initialSpawnCount);

        for (int index = 1; index < initialSpawnCount; index++)
        {
            GameObject clone = Instantiate(gameObject);
            BendicionBehaviour cloneBehaviour = clone.GetComponent<BendicionBehaviour>();
            cloneBehaviour.initialSpawnCount = 1;
        }

        SpawnOnRandomSurface();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();

        if (player == null || !IsVisible())
        {
            return;
        }

        player.ActivateShield(shieldDuration);
        SetVisible(false);
        CancelInvoke(nameof(Respawn));
        Invoke(nameof(Respawn), Random.Range(minRespawnDelay, maxRespawnDelay));
    }

    private void Respawn()
    {
        SpawnOnRandomSurface();
    }

    private void SpawnOnRandomSurface()
    {
        Collider2D[] colliders = FindObjectsOfType<Collider2D>();
        List<Collider2D> surfaces = new List<Collider2D>();
        Bounds spawnBounds = new Bounds(spawnAreaCenter, spawnAreaSize);

        foreach (Collider2D candidate in colliders)
        {
            if (candidate != pickupCollider && candidate.CompareTag(groundTag) &&
                candidate.bounds.Intersects(spawnBounds))
            {
                surfaces.Add(candidate);
            }
        }

        if (surfaces.Count == 0)
        {
            Debug.LogWarning($"No se encontro una superficie con tag '{groundTag}' para Bendicion.");
            return;
        }

        Collider2D surface = surfaces[Random.Range(0, surfaces.Count)];

        float pickupHalfWidth = pickupCollider.bounds.extents.x;
        float minX = spawnBounds.min.x + pickupHalfWidth + surfacePadding;
        float maxX = spawnBounds.max.x - pickupHalfWidth - surfacePadding;
        float x;
        Vector2 surfacePoint;

        if (!TryFindSurfacePoint(surface, minX, maxX, out x, out surfacePoint))
        {
            Debug.LogWarning($"No se encontro una superficie solida para Bendicion en '{surface.name}'.");
            return;
        }

        Bounds currentPickupBounds = pickupCollider.bounds;
        Vector3 position = transform.position;
        position.x += surfacePoint.x - currentPickupBounds.center.x;
        position.y += surfacePoint.y - currentPickupBounds.min.y;
        transform.position = position;
        SetVisible(true);
    }

    private bool TryFindSurfacePoint(Collider2D surface, float minX, float maxX, out float x, out Vector2 surfacePoint)
    {
        Bounds areaBounds = new Bounds(spawnAreaCenter, spawnAreaSize);
        Bounds surfaceBounds = surface.bounds;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            x = minX <= maxX ? Random.Range(minX, maxX) : areaBounds.center.x;
            Vector2 rayOrigin = new Vector2(x, surfaceBounds.max.y + 1f);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, surfaceBounds.size.y + 2f);

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == surface)
                {
                    surfacePoint = hit.point;
                    return true;
                }
            }
        }

        x = areaBounds.center.x;
        surfacePoint = Vector2.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(spawnAreaCenter, spawnAreaSize);
    }

    private bool IsVisible()
    {
        return pickupRenderer == null || pickupRenderer.enabled;
    }

    private void SetVisible(bool isVisible)
    {
        if (pickupRenderer != null)
        {
            pickupRenderer.enabled = isVisible;
        }

        if (pickupCollider != null)
        {
            pickupCollider.enabled = isVisible;
        }
    }
}
