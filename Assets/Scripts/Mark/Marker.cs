using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Marker : MonoBehaviour
{
    [Header("Debounce Settings")]
    [Tooltip("Minimum linear movement before adding a new point.")]
    public float minDistance = 0.05f;

    [Tooltip("Minimum angle change (degrees) to add a new point even if distance is small.")]
    public float minAngleDegrees = 5f;

    [Header("Simplification")]
    [Tooltip("Tolerance for final line simplification after ending the stroke.")]
    public float simplifyTolerance = 0.02f;

    [Tooltip("Optional: maximum number of points the line keeps. Set <= 0 for unlimited.")]
    public int maxPoints = 0;

    [Header("Target")]
    [Tooltip("Leg transform to track while marking.")]
    public Transform legToTrack;

    [Tooltip("Fixed Y position for line points.")]
    public float fixedYpos = 0.1f;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 lastPoint;
    private Vector3 lastDirection = Vector3.zero;
    private bool isMarkingActive = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ResetTrace();
    }

    void Update()
    {
        if (!isMarkingActive || legToTrack == null)
            return;

        Vector3 current = new Vector3(legToTrack.position.x, fixedYpos, legToTrack.position.z);

        if (points.Count == 0)
        {
            AddPointDirect(current);
        }
        else
        {
            float dist = Vector3.Distance(lastPoint, current);
            Vector3 newDir = (current - lastPoint).normalized;

            bool movedEnough = dist >= minDistance;
            bool turnedEnough = false;

            if (points.Count >= 2)
            {
                float angle = Vector3.Angle(lastDirection, newDir);
                turnedEnough = angle >= minAngleDegrees;
            }
            else
            {
                turnedEnough = movedEnough;
            }

            if (movedEnough || turnedEnough)
            {
                AddPointDirect(current);
                lastDirection = newDir;
            }
        }
    }

    private void AddPointDirect(Vector3 p)
    {
        if (maxPoints > 0 && points.Count >= maxPoints)
        {
            points.RemoveAt(0);
        }

        points.Add(p);
        lastPoint = p;
        UpdateLine();
    }

    private void UpdateLine()
    {
        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());
    }

    /// <summary>
    /// Called externally (e.g., from OnStartDragLegA/B) to begin marking.
    /// </summary>
    public void OnStartMarking()
    {
        isMarkingActive = true;
        points.Clear();

        if (legToTrack != null)
        {
            Vector3 startPos = new Vector3(legToTrack.position.x, fixedYpos, legToTrack.position.z);
            AddPointDirect(startPos);
        }
    }

    /// <summary>
    /// Called externally (e.g., from OnEndDragA/B) to end marking.
    /// </summary>
    public void RunSimplify()
    {
        isMarkingActive = false;
        if (simplifyTolerance > 0f && points.Count > 2)
            lineRenderer.Simplify(simplifyTolerance);
    }

    public void ResetTrace()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
        lastDirection = Vector3.zero;
        isMarkingActive = false;
    }
}
