using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class TwoLegRevolver : MonoBehaviour
{
    [Header("Legs")]
    public Transform legA;
    public Transform legB;

    [Header("Settings")]
    public float orbitRadius = 1f; // radius of circular track
    public float pickRadius = 0.3f; // how close you must click to grab a leg

    [Header("Events")]
    public UnityEvent OnStartDragLegA;
    public UnityEvent OnStartDragLegB;
    public UnityEvent OnEndDragA;
    public UnityEvent OnEndDragB;

    // internal
    private Transform draggingLeg = null;
    private Transform fixedAnchor = null;
    private bool isDragging = false;

    private Plane dragPlane;

    void OnEnable()
    {
        SetupDragPlane();
        if (legA != null && legB != null)
        {
            if ((legA.position - legB.position).magnitude < 0.001f)
            {
                legA.position = transform.position + Vector3.right * orbitRadius * 0.5f;
                legB.position = transform.position + Vector3.left * orbitRadius * 0.5f;
            }
            RecenterParent();
        }
    }

    void Update()
    {
        if (legA == null || legB == null) return;

        HandleInput();
        RecenterParent();
    }

    void SetupDragPlane()
    {
        Camera cam = Camera.main;
        if (cam != null)
            dragPlane = new Plane(cam.transform.forward * -1f, transform.position);
        else
            dragPlane = new Plane(Vector3.forward, transform.position);
    }

    void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (PointerOverUI()) return;

            Vector3 worldPos;
            if (!UpdateDragPlaneForCurrentView()) return;
            if (ScreenPointToPlane(Input.mousePosition, out worldPos))
            {
                float dA = Vector3.Distance(worldPos, legA.position);
                float dB = Vector3.Distance(worldPos, legB.position);

                if (dA <= pickRadius && dA <= dB)
                {
                    StartDrag(legA, legB);
                    OnStartDragLegA?.Invoke();
                }
                else if (dB <= pickRadius && dB < dA)
                {
                    StartDrag(legB, legA);
                    OnStartDragLegB?.Invoke();
                }
            }
        }

        if (Input.GetMouseButton(0) && isDragging && draggingLeg != null && fixedAnchor != null)
        {
            Vector3 worldPos;
            if (ScreenPointToPlane(Input.mousePosition, out worldPos))
            {
                Vector3 dir = (worldPos - fixedAnchor.position);
                if (dir.sqrMagnitude < 0.0001f) return;
                dir = dir.normalized;
                Vector3 target = fixedAnchor.position + dir * orbitRadius;
                draggingLeg.position = target;
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            if (draggingLeg == legA)
            {
                Debug.Log("OnEndDrag A");
                OnEndDragA?.Invoke();
            }
            else if (draggingLeg == legB)
            {
                Debug.Log("OnEndDrag B");
                OnEndDragB?.Invoke();
            }


            EndDrag();
        }
    }

    bool UpdateDragPlaneForCurrentView()
    {
        Camera cam = Camera.main;
        if (cam == null) return false;
        if (fixedAnchor != null)
            dragPlane = new Plane(cam.transform.forward * -1f, fixedAnchor.position);
        else
            dragPlane = new Plane(cam.transform.forward * -1f, transform.position);
        return true;
    }

    void StartDrag(Transform toDrag, Transform anchor)
    {
        draggingLeg = toDrag;
        fixedAnchor = anchor;
        isDragging = true;
        UpdateDragPlaneForCurrentView();
        Vector3 fromAnchor = (draggingLeg.position - fixedAnchor.position).normalized;
        if (fromAnchor.sqrMagnitude < 0.0001f)
            fromAnchor = Vector3.right;
        draggingLeg.position = fixedAnchor.position + fromAnchor * orbitRadius;
    }

    void EndDrag()
    {
        draggingLeg = null;
        fixedAnchor = null;
        isDragging = false;
    }

    bool PointerOverUI()
    {
        if (EventSystem.current != null)
            return EventSystem.current.IsPointerOverGameObject();
        return false;
    }

    bool ScreenPointToPlane(Vector3 screenPoint, out Vector3 worldPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float enter;
        if (dragPlane.Raycast(ray, out enter))
        {
            worldPos = ray.GetPoint(enter);
            return true;
        }
        worldPos = Vector3.zero;
        return false;
    }

    void RecenterParent()
    {
        if (legA == null || legB == null) return;
        Vector3 mid = (legA.position + legB.position) * 0.5f;
        Vector3 delta = mid - transform.position;
        if (delta.sqrMagnitude < 1e-8f) return;
        transform.position = mid;
        legA.position -= delta;
        legB.position -= delta;
    }

    void OnDrawGizmos()
    {
        if (legA != null && legB != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(legA.position, orbitRadius);
            Gizmos.DrawWireSphere(legB.position, orbitRadius);

            Gizmos.color = Color.green;
            Gizmos.DrawSphere(legA.position, 0.05f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(legB.position, 0.05f);

            if (draggingLeg != null && fixedAnchor != null)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawLine(fixedAnchor.position, draggingLeg.position);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, 0.04f);
        }
    }
}
