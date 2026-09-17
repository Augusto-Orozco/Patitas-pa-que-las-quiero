using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Collider2D levelBounds;
    [SerializeField] private bool boundsAreTrigger = true;
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothTime = 0.15f;

    private Camera cameraComponent;
    private Vector3 followVelocity;
    private bool hasSnappedToTarget;

    private void Awake()
    {
        cameraComponent = GetComponent<Camera>();
    }

    private void Start()
    {
        if (levelBounds != null && boundsAreTrigger)
        {
            levelBounds.isTrigger = true;
        }

        FindTarget();
    }

    private void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }

        Vector3 targetPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z);

        if (!hasSnappedToTarget)
        {
            transform.position = ClampToLevel(targetPosition);
            followVelocity = Vector3.zero;
            hasSnappedToTarget = true;
            return;
        }

        Vector3 nextPosition = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref followVelocity,
            smoothTime);

        transform.position = ClampToLevel(nextPosition);
    }

    private void FindTarget()
    {
        PlayerMovement player = FindObjectOfType<PlayerMovement>();

        if (player != null)
        {
            target = player.transform;
            hasSnappedToTarget = false;
        }
    }

    private Vector3 ClampToLevel(Vector3 position)
    {
        if (levelBounds == null)
        {
            return position;
        }

        float verticalExtent = cameraComponent.orthographicSize;
        float horizontalExtent = verticalExtent * cameraComponent.aspect;
        Bounds bounds = levelBounds.bounds;

        float minX = bounds.min.x + horizontalExtent;
        float maxX = bounds.max.x - horizontalExtent;
        float minY = bounds.min.y + verticalExtent;
        float maxY = bounds.max.y - verticalExtent;

        if (minX > maxX)
        {
            position.x = bounds.center.x;
        }
        else
        {
            position.x = Mathf.Clamp(position.x, minX, maxX);
        }

        if (minY > maxY)
        {
            position.y = bounds.center.y;
        }
        else
        {
            position.y = Mathf.Clamp(position.y, minY, maxY);
        }

        return position;
    }
}
