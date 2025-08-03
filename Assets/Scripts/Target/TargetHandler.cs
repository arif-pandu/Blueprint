using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TargetHandler : StaticReference<TargetHandler>
{
    [SerializeField] private float tolerance = 0.5f;
    [SerializeField] private List<Target> targets = new();
    [SerializeField] private Transform parentToSpawn;

    [Header("Events")]
    [SerializeField] private UnityEvent allTargetsActiveEvent;

    public List<Target> Targets
    {
        get { return targets; }
    }

    private List<LineRenderer> lineRenderers = new();

    // track to avoid firing the event repeatedly if nothing changed
    private bool hadAllActive = false;

    void Awake()
    {
        BaseAwake(this);
    }

    void OnDestroy()
    {
        BaseOnDestroy();
    }

    public void SpawnTargets(Level levelObj)
    {
        Level theObj = Instantiate(levelObj);
        theObj.transform.SetParent(parentToSpawn);
        theObj.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        theObj.transform.localScale = Vector3.one; // Reset scale to parent

        RegisterTargets(theObj);
    }

    public void RegisterTargets(Level levelData)
    {
        if (levelData.Targets == null || levelData.Targets.Count == 0)
        {
            Debug.LogWarning("Attempted to register an empty or null list of targets.");
            return;
        }

        foreach (var target in levelData.Targets)
        {
            if (target != null && !targets.Contains(target))
            {
                targets.Add(target);
            }
        }

        // Reset the all-active tracking when new targets are added
        hadAllActive = false;
    }

    public void RegisterLineRenderer(LineRenderer lineRenderer)
    {
        if (lineRenderer == null)
        {
            Debug.LogWarning("Attempted to register a null LineRenderer.");
            return;
        }

        if (!lineRenderers.Contains(lineRenderer))
        {
            lineRenderers.Add(lineRenderer);
            Debug.Log($"Registered LineRenderer: {lineRenderer.name}");
        }
    }

    public void OnMarkerFinishMarking()
    {
        CheckIntersection(lineRenderers);
    }

    public void CheckIntersection(List<LineRenderer> theLine)
    {
        if (targets == null || targets.Count == 0) return;

        foreach (var target in targets)
        {
            if (target == null) continue;

            bool intersects = false;
            Vector3 targetPos = target.transform.position;

            foreach (LineRenderer line in theLine)
            {
                if (line == null) continue;

                int pointCount = line.positionCount;
                for (int i = 0; i < pointCount - 1; i++)
                {
                    Vector3 start = line.GetPosition(i);
                    Vector3 end = line.GetPosition(i + 1);
                    Vector3 segment = end - start;

                    float t = 0f;
                    float denom = Vector3.SqrMagnitude(segment);
                    if (denom > Mathf.Epsilon)
                    {
                        t = Vector3.Dot(targetPos - start, segment) / denom;
                        t = Mathf.Clamp01(t);
                    }

                    Vector3 closestPoint = start + segment * t;
                    float distance = Vector3.Distance(targetPos, closestPoint);

                    if (distance < tolerance)
                    {
                        intersects = true;
                        break; // no need to check other segments for this target
                    }
                }

                if (intersects) break;
            }

            if (intersects)
            {
                if (!target.IsActive)
                {
                    target.SetMaterialByState(true);
                    target.IsActive = true;
                }
            }
            else
            {
                if (target.IsActive)
                {
                    target.SetMaterialByState(false);
                    target.IsActive = false;
                }
            }
        }

        // After updating all targets, check if all are active
        bool allActiveNow = true;
        foreach (var t in targets)
        {
            if (t == null || !t.IsActive)
            {
                allActiveNow = false;
                Debug.Log($"Target is not active.");
                break;
            }
        }

        if (allActiveNow && !hadAllActive)
        {
            // invoke the event once upon transition to all-active
            Debug.Log("All targets are now active.");
            allTargetsActiveEvent?.Invoke();
        }

        hadAllActive = allActiveNow;
    }

    void OnDrawGizmos()
    {
        if (targets == null || targets.Count == 0) return;

        Gizmos.color = Color.red;
        foreach (var target in targets)
        {
            if (target != null)
            {
                Gizmos.DrawWireSphere(target.transform.position, tolerance);
            }
        }
    }
}
