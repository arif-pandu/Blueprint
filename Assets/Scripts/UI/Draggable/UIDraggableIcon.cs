using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public GameObject worldPrefab;


    [Header("Snap")]
    public bool isSnapping = true;
    public float snapSize = 1f;

    [Header("Animation")]
    [SerializeField] private float scaleDownSize = 0.8f; // Scale down to 80% when dragging
    [SerializeField] private float scaleDownDuration = 0.15f;
    [SerializeField] private LeanTweenType scaleDownEase = LeanTweenType.easeOutQuad;
    [SerializeField] private float scaleUpDuration = 0.15f;
    [SerializeField] private LeanTweenType scaleUpEase = LeanTweenType.easeOutQuad;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        canvasGroup.blocksRaycasts = false;

        // Animate scale down when dragging starts
        AnimateScaleDown(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Animate scale back up when dragging ends
        AnimateScaleDown(false);

        canvasGroup.blocksRaycasts = true;

        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("No Main Camera found for raycast.");
            AnimateDestroyIcon();
            return;
        }

        Ray ray = cam.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (isSnapping)
            {
                Vector3 snapped = SnapToGrid(hit.point, snapSize);
                SpawnObject(snapped);
            }
            else
            {
                SpawnObject(hit.point);
            }

        }

        AnimateDestroyIcon();
    }

    private void AnimateScaleDown(bool isDrag)
    {
        // scale down when isDrag is true
        // scale back to Vector3.one when isDrag is false
        float duration = isDrag ? scaleDownDuration : scaleUpDuration;
        LeanTween.scale(gameObject, isDrag ? Vector3.one * scaleDownSize : Vector3.one, duration)
            .setEase(isDrag ? scaleDownEase : scaleUpEase)
            .setOnComplete(() =>
            {
                if (!isDrag)
                {
                    canvasGroup.blocksRaycasts = true;
                }
            });
    }

    private Vector3 SnapToGrid(Vector3 position, float gridSize)
    {
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            Mathf.Round(position.z / gridSize) * gridSize
        );
    }

    private void AnimateDestroyIcon()
    {
        LeanTween.scale(
            gameObject, Vector3.zero, 0.15f)
            .setEase(LeanTweenType.easeOutQuad)
            .setOnStart(() => DraggableIconHandler.Instance.spawnedIcons.Remove(this))
            .setOnComplete(() => Destroy(gameObject));
    }

    private void SpawnObject(Vector3 pos)
    {
        CompassHandler.Instance.SpawnCompassAtWorldPos(pos);
    }
}
