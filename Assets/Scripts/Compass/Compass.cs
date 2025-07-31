using UnityEngine;

public class Compass : MonoBehaviour
{
    [Header("Animation")]
    public float moveDuration = 0.2f;
    public LeanTweenType moveEase = LeanTweenType.easeInOutSine;


    [Header("Setup")]
    public LayerMask raycastLayers;
    public bool debugDraw = true;
    public bool preserveY = true;


    [Header("State")]
    private Camera cam;
    private int currentTweenId = -1;


    void Awake()
    {
        cam = Camera.main;
        if (cam == null)
            Debug.LogWarning("[Compass] No Main Camera found. Assign your camera with tag MainCamera.");
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TrySetDestination(Input.mousePosition);
        }

        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
            {
                TrySetDestination(t.position);
            }
        }
    }

    void TrySetDestination(Vector2 screenPos)
    {
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        Vector3 targetWorld;

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers, QueryTriggerInteraction.Ignore))
        {
            targetWorld = hit.point;
        }
        else
        {
            Plane ground = new Plane(Vector3.up, Vector3.zero);
            if (ground.Raycast(ray, out float enter))
            {
                targetWorld = ray.GetPoint(enter);
            }
            else
            {
                return;
            }
        }

        if (preserveY)
            targetWorld.y = transform.position.y;

        if (debugDraw)
        {
            Debug.DrawLine(ray.origin, targetWorld, Color.green, 1f);
            Debug.DrawRay(targetWorld, Vector3.up * 0.1f, Color.yellow, 1f);
        }

        // Cancel previous tween if running
        if (currentTweenId != -1)
        {
            LeanTween.cancel(currentTweenId);
            currentTweenId = -1;
        }

        // Start new tween
        currentTweenId = LeanTween.move(gameObject, targetWorld, moveDuration).setEase(moveEase).id;
    }
}
