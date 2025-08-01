using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(LineRenderer))]
public class Marker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CompassRotator rotator;

    [Header("Debounce Settings")]
    [Tooltip("Minimum linear movement before considering a new point.")]
    public float minDistance = 0.05f;

    [Tooltip("Minimum angle change (degrees) to add a new point even if distance is small.")]
    public float minAngleDegrees = 5f;

    [Header("Simplification")]
    [Tooltip("Tolerance for final Ramer-Douglas-Peucker simplification after release.")]
    public float simplifyTolerance = 0.02f;

    [Tooltip("Optional: maximum number of points the line keeps. Set <= 0 for unlimited.")]
    public int maxPoints = 0;

    [Tooltip("If true, allows clearing the trace with R key.")]
    public bool allowResetWithR = true;

    private LineRenderer lineRenderer;
    private List<Vector3> points = new List<Vector3>();
    private Vector3 lastPoint;
    private Vector3 lastDirection = Vector3.zero;
    private bool isDragging = false;
    private bool isMarkingActive = false;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        ResetTrace();
    }

    void Update()
    {
        if (!isMarkingActive)
            return;

        Vector3 current = transform.position;

        // Start stroke on mouse/touch begin
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            points.Clear();
            AddPointDirect(current);
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            // Debounced point addition while dragging
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
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            // Finish stroke and simplify
            isDragging = false;
            RunSimplify();
        }

        if (allowResetWithR && Input.GetKeyDown(KeyCode.R))
        {
            ResetTrace();
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
    /// Call this when CompassRotator.OnEndDrag
    /// </summary>
    public void RunSimplify()
    {
        isMarkingActive = false;
        lineRenderer.Simplify(simplifyTolerance);
    }

    /// <summary>
    ///  call this when CompassRotator.OnStartDrag
    /// </summary>
    public void OnStartMarking()
    {
        isMarkingActive = true;
    }

    public void ResetTrace()
    {
        points.Clear();
        lineRenderer.positionCount = 0;
        lastPoint = transform.position;
        lastDirection = Vector3.zero;
        isDragging = false;
    }

}
