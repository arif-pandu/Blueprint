using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LineIntersectionChecker : MonoBehaviour
{
    [Header("Source")]
    public LineRenderer lineRenderer; // if left null, will try to auto-assign from this GameObject

    [Header("Targets")]
    [Tooltip("Explicit targets to test against. Their position is used with targetRadius.")]
    public List<Target> explicitTargets = new();

    [Tooltip("Optional: instead of using explicitTargets, you can supply a LayerMask and it will query nearby colliders each frame.")]
    public LayerMask targetLayerMask;
    public float targetRadius = 0.2f; // sphere-approximation radius per target when using explicitTargets or overlap query

    [Header("Optimization")]
    [Tooltip("If true, the line's positions are cached and only refreshed when the LineRenderer has changed (cheap heuristic).")]
    public bool cachePositions = true;

    [Header("Events")]
    public UnityEvent OnAnyTargetIntersected; // invoked once per check if any intersection is found

    // read-only result (for external polling)
    [System.NonSerialized] public bool anyIntersectionThisFrame = false;

    // internal
    private Vector3[] cachedPositions = new Vector3[0];
    private int lastPositionCount = -1;
    private float lastStartWidth = -1f;
    private Vector3 lastTransformScale = Vector3.one;

    void Reset()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        anyIntersectionThisFrame = CheckAnyIntersected();
        if (anyIntersectionThisFrame)
        {
            OnAnyTargetIntersected?.Invoke();
        }
    }

    public bool CheckAnyIntersected()
    {
        if (lineRenderer == null) return false;

        // Fetch and cache positions if needed
        int count = lineRenderer.positionCount;
        if (count < 2) return false;

        if (!cachePositions || cachedPositions.Length != count || LineChanged())
        {
            EnsureCapacity(ref cachedPositions, count);
            lineRenderer.GetPositions(cachedPositions);
            lastPositionCount = count;
            lastStartWidth = lineRenderer.startWidth;
            lastTransformScale = transform.lossyScale;
        }

        // Broad-phase: use the LineRenderer.bounds expanded by max target radius as quick reject if using explicit targets
        // We'll test each target individually with early exit
        // First, gather targets from overlap if no explicitTargets provided
        if (explicitTargets != null && explicitTargets.Count > 0)
        {
            foreach (var t in explicitTargets)
            {
                if (t == null) continue;
                if (IsTargetIntersectingPolyline(t.transform.position, targetRadius, cachedPositions))
                    return true;
            }
        }
        else
        {
            // use physics overlap to gather nearby candidate colliders (approximate target centers from their bounds)
            // We'll sample the line's bounds once:
            Bounds lineBounds = lineRenderer.bounds;
            float expanded = targetRadius;
            lineBounds.Expand(expanded * 2f);
            Collider[] hits = Physics.OverlapBox(lineBounds.center, lineBounds.extents, Quaternion.identity, targetLayerMask, QueryTriggerInteraction.Ignore);
            foreach (var col in hits)
            {
                if (col == null) continue;
                Vector3 targetPos = col.ClosestPoint(col.bounds.center); // approximate center
                if (IsTargetIntersectingPolyline(targetPos, targetRadius, cachedPositions))
                    return true;
            }
        }

        return false;
    }

    private bool LineChanged()
    {
        // simple heuristic: width or scale changed (could affect world-space thickness if you care)
        if (!Mathf.Approximately(lastStartWidth, lineRenderer.startWidth)) return true;
        if (transform.lossyScale != lastTransformScale) return true;
        return false;
    }

    private static void EnsureCapacity(ref Vector3[] arr, int needed)
    {
        if (arr == null || arr.Length < needed)
            arr = new Vector3[needed];
    }

    private bool IsTargetIntersectingPolyline(Vector3 targetCenter, float radius, Vector3[] polyline)
    {
        float threshold = radius; // assuming line has no thickness; if you want to include line width, add half-width here
        float thresholdSqr = threshold * threshold;

        // Optional cheap broad-phase: if the target is far from the line's bounds
        // (you can skip if you trust the number of targets is small)

        for (int i = 0; i < polyline.Length - 1; i++)
        {
            float dist2 = SquaredDistancePointToSegment_XZ(targetCenter, polyline[i], polyline[i + 1]);

            if (dist2 <= thresholdSqr)
                return true; // early out
        }

        return false;
    }

    // utility: squared distance from point to segment AB
    public static float SquaredDistancePointToSegment_XZ(Vector3 point, Vector3 a, Vector3 b)
    {
        Vector2 p = new Vector2(point.x, point.z);
        Vector2 a2 = new Vector2(a.x, a.z);
        Vector2 b2 = new Vector2(b.x, b.z);

        Vector2 ab = b2 - a2;
        Vector2 ap = p - a2;
        float abLen2 = Vector2.Dot(ab, ab);
        if (abLen2 <= Mathf.Epsilon)
            return (p - a2).sqrMagnitude; // degenerate

        float t = Vector2.Dot(ap, ab) / abLen2;
        t = Mathf.Clamp01(t);
        Vector2 closest = a2 + ab * t;
        return (p - closest).sqrMagnitude;
    }


    void OnDrawGizmosSelected()
    {
        if (lineRenderer == null) return;
        Gizmos.color = anyIntersectionThisFrame ? Color.red : Color.green;

        // draw simplified polyline
        int count = lineRenderer.positionCount;
        if (count < 2) return;

        Vector3[] temp = new Vector3[count];
        lineRenderer.GetPositions(temp);
        for (int i = 0; i < count - 1; i++)
        {
            Gizmos.DrawLine(temp[i], temp[i + 1]);
        }

        // draw explicit target spheres
        if (explicitTargets != null)
        {
            Gizmos.color = Color.yellow;
            foreach (var t in explicitTargets)
            {
                if (t == null) continue;
                Gizmos.DrawWireSphere(t.transform.position, targetRadius);
            }
        }
    }
}
