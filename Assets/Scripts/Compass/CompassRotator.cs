using UnityEngine;

/// <summary>
/// Drag this handler to rotate its parent (statue) around Y.
/// The handler stays at its local offset; dragging computes the angle delta
/// on the XZ plane and applies it to the statue's Y rotation.
/// Requires a Collider on the handler for mouse events.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CompassRotator : MonoBehaviour
{
    [Tooltip("The statue/parent transform to rotate. If null, uses parent.")]
    public Transform statue;

    private Camera cam;
    private bool isDragging = false;

    private Plane groundPlane; // XZ plane at statue's Y
    private Vector3 initialDir; // from statue to drag point on plane
    private float initialStatueYAngle;

    void Start()
    {
        cam = Camera.main;
        if (statue == null)
        {
            if (transform.parent != null)
                statue = transform.parent;
            else
                Debug.LogError("HandlerDrivenYRotate: Statue not assigned and handler has no parent.");
        }
    }

    void OnMouseDown()
    {
        if (statue == null) return;
        isDragging = true;

        // define XZ plane at statue's current Y
        float y = statue.position.y;
        groundPlane = new Plane(Vector3.up, new Vector3(0, y, 0));

        // Raycast from mouse to plane to get initial point
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            initialDir = hit - statue.position;
            initialDir.y = 0f;
            if (initialDir.sqrMagnitude < 0.0001f)
                initialDir = Vector3.forward; // fallback
        }

        initialStatueYAngle = statue.eulerAngles.y;
    }

    void OnMouseDrag()
    {
        if (!isDragging || statue == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hit = ray.GetPoint(enter);
            Vector3 currentDir = hit - statue.position;
            currentDir.y = 0f;
            if (currentDir.sqrMagnitude < 0.0001f) return;

            // Compute signed angle from initialDir to currentDir around up
            float angleDelta = Vector3.SignedAngle(initialDir.normalized, currentDir.normalized, Vector3.up);
            float newY = initialStatueYAngle + angleDelta;

            Vector3 euler = statue.eulerAngles;
            euler.y = newY;
            statue.eulerAngles = euler;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }
}
