using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    public GameObject worldPrefab;


    [Header("Snap")]
    public bool isSnapping = true;
    public float snapSize = 1f;

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
