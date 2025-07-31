using UnityEngine;

public class Compass : MonoBehaviour
{
    [Header("Rig")]
    [SerializeField] private CompassRig rig;

    [Header("Animation")]
    public LeanTweenType spawnEase = LeanTweenType.easeInOutSine;
    public float durationSpawnAnim = 0.125f;

    [Header("Setup")]
    public LayerMask raycastLayers;
    public bool preserveY = true;
    [Tooltip("Multiplier from screen-space delta to world X movement.")]
    [SerializeField] private float dragSensitivity = 0.01f;

    [Header("State")]
    private Camera cam;
    private int currentTweenId = -1;

    // internal drag state
    private bool isDragging = false;
    private float initialAffectedAX;
    private Vector3 dragStartMousePos;

    public void AnimateSpawn()
    {
        LeanTween.cancel(currentTweenId);
        currentTweenId = LeanTween.scale(gameObject, Vector3.one, durationSpawnAnim)
            .setEase(spawnEase)
            .setOnComplete(() => currentTweenId = -1)
            .id;
    }

    void Awake()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (rig == null || rig.theAffectedA == null)
            return;

        // Begin drag
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, raycastLayers))
            {
                if (hit.collider != null && hit.collider.gameObject == gameObject)
                {
                    isDragging = true;
                    dragStartMousePos = Input.mousePosition;
                    // Store the initial x (in local space to preserve transform hierarchy behavior)
                    initialAffectedAX = rig.theAffectedA.transform.localPosition.x;
                }
            }
        }

        // Dragging
        if (isDragging && Input.GetMouseButton(0))
        {
            float deltaScreenX = Input.mousePosition.x - dragStartMousePos.x;
            float deltaWorldX = deltaScreenX * dragSensitivity;

            Vector3 localPos = rig.theAffectedA.transform.localPosition;
            localPos.x = initialAffectedAX + deltaWorldX;
            rig.theAffectedA.transform.localPosition = localPos;
        }

        // End drag
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}
