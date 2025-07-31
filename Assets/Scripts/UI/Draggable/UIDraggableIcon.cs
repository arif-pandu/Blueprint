using UnityEngine;
using UnityEngine.EventSystems;

public class UIDraggableIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("Prefab to spawn into world space on drop.")]
    public GameObject worldPrefab;

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    // private Vector2 originalPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return; // ignore non-left-button drags

        // originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false; // so drop can see underlying raycast if needed
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        // Move the UI icon with the pointer
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        canvasGroup.blocksRaycasts = true;

        // Raycast from screen point into world
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            SpawnObject(hit.point);
        }

        AnimateDestroyIcon();
    }

    private void AnimateDestroyIcon()
    {
        // Optionally destroy the icon if you don't want it to remain in the UI
        // and remove from the list in DraggableIconHandler
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
