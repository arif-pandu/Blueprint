using UnityEngine;

/// <summary>
/// Keeps this object at fixed distances to two targets simultaneously.
/// If both distance constraints are compatible, the object stays on the circle
/// where the two spheres intersect, biased to the highest-Y solution. Otherwise
/// it falls back to enforcing the first target's distance, also biased upward.
/// Additionally enforces that the two targets themselves are no farther apart
/// than `fixedDistanceTwoCircle`.
/// </summary>
[ExecuteAlways]
public class CompassRig : MonoBehaviour
{
    [Header("Targets / Distances")]
    public GameObject theAffectedA;
    public GameObject theAffectedB;

    [Tooltip("Desired fixed distance to A. If <= 0, captures initial at Start.")]
    public float fixedDistanceA = 0f;
    [Tooltip("Desired fixed distance to B. If <= 0, captures initial at Start.")]
    public float fixedDistanceB = 0f;

    [Header("Target Pair Separation Limit")]
    [Tooltip("Maximum allowed distance between theAffectedA and theAffectedB. If exceeded, they are pulled toward midpoint.")]
    public float fixedDistanceTwoCircle = 0f; // <=0 means no clamp

    [Header("Smoothing")]
    [Tooltip("1 = instant snap, <1 = interpolation toward the constrained position.")]
    [Range(0f, 1f)]
    public float positionLerp = 1f;

    [Header("Collider")]
    [SerializeField] private BoxCollider rigCollider;


    [Header("Pencil and Needle")]
    public GameObject anchorPencil;
    public GameObject anchorNeedle;


    [Header("State")]
    private float effectiveDistanceA;
    private float effectiveDistanceB;
    private bool initialized = false;

    void Start()
    {
        ResetRigPose();
        Initialize();
    }

    void OnValidate()
    {
        if (!Application.isPlaying)
            Initialize();
    }

    private void ResetRigPose()
    {
        theAffectedA.transform.localPosition = Vector3.zero;
        theAffectedB.transform.localPosition = Vector3.zero;
        transform.localPosition = new Vector3(0f, 3f, 0f);
    }

    private void Initialize()
    {
        if (theAffectedA != null)
        {
            Vector3 deltaA = transform.position - theAffectedA.transform.position;
            effectiveDistanceA = (fixedDistanceA > 0f) ? fixedDistanceA : deltaA.magnitude;
        }

        if (theAffectedB != null)
        {
            Vector3 deltaB = transform.position - theAffectedB.transform.position;
            effectiveDistanceB = (fixedDistanceB > 0f) ? fixedDistanceB : deltaB.magnitude;
        }

        initialized = true;
    }

    void Update()
    {
        if (!initialized)
            Initialize();

        // Clamp separation between theAffectedA and theAffectedB if needed
        if (theAffectedA != null && theAffectedB != null && fixedDistanceTwoCircle > 0f)
        {
            Vector3 posA = theAffectedA.transform.position;
            Vector3 posB = theAffectedB.transform.position;
            float currentDist = Vector3.Distance(posA, posB);
            if (currentDist > fixedDistanceTwoCircle)
            {
                Vector3 midpoint = (posA + posB) * 0.5f;
                Vector3 dirA = (posA - midpoint).normalized;
                Vector3 dirB = (posB - midpoint).normalized;

                Vector3 newA = midpoint + dirA * (fixedDistanceTwoCircle * 0.5f);
                Vector3 newB = midpoint + dirB * (fixedDistanceTwoCircle * 0.5f);

                theAffectedA.transform.position = newA;
                theAffectedB.transform.position = newB;
            }
        }

        if (theAffectedA == null)
            return;

        Vector3 desiredPos = transform.position;
        Vector3 posA_current = theAffectedA.transform.position;

        if (theAffectedB != null)
        {
            Vector3 posB_current = theAffectedB.transform.position;
            float d = Vector3.Distance(posA_current, posB_current);
            float r0 = effectiveDistanceA;
            float r1 = effectiveDistanceB;

            if (d > Mathf.Epsilon && d <= r0 + r1 && d >= Mathf.Abs(r0 - r1))
            {
                // Compute intersection circle
                Vector3 ex = (posB_current - posA_current).normalized;
                float x = (d * d - r1 * r1 + r0 * r0) / (2f * d);
                Vector3 circleCenter = posA_current + ex * x;
                float sq = r0 * r0 - x * x;
                float circleRadius = sq > 0f ? Mathf.Sqrt(sq) : 0f;

                // Build orthonormal basis perpendicular to ex
                Vector3 arbitrary = Vector3.Cross(ex, Vector3.up);
                if (arbitrary.sqrMagnitude < 1e-6f)
                    arbitrary = Vector3.Cross(ex, Vector3.right);
                Vector3 ey = Vector3.Cross(ex, arbitrary).normalized;
                Vector3 ez = Vector3.Cross(ex, ey).normalized;

                // Choose point on circle with maximum world Y:
                // maximize (cos t * ey.y + sin t * ez.y)
                float A = ey.y;
                float B = ez.y;
                float t = Mathf.Atan2(B, A); // angle giving max of A cos t + B sin t
                Vector3 bestDir = Mathf.Cos(t) * ey + Mathf.Sin(t) * ez;
                desiredPos = circleCenter + bestDir.normalized * circleRadius;
            }
            else
            {
                // fallback to single A with upward bias
                desiredPos = EnforceSingleSphereUpwardBiased(posA_current, effectiveDistanceA, transform.position);
            }
        }
        else
        {
            desiredPos = EnforceSingleSphereUpwardBiased(posA_current, effectiveDistanceA, transform.position);
        }

        ApplyPosition(desiredPos);
        RotateAnchorZLookAtBase();
        ColliderFollowCenter();
    }

    private Vector3 EnforceSingleSphereUpwardBiased(Vector3 center, float radius, Vector3 current)
    {
        Vector3 dir = current - center;
        if (dir.sqrMagnitude < 1e-6f)
        {
            dir = Vector3.up * radius;
        }

        // Bias upward: if resulting point would be below the anchor, flip Y of direction to be positive
        if (dir.y < 0f)
            dir.y = -dir.y;

        return center + dir.normalized * radius;
    }

    private void ApplyPosition(Vector3 targetPos)
    {
        if (positionLerp >= 1f)
            transform.position = targetPos;
        else
            transform.position = Vector3.Lerp(transform.position, targetPos, positionLerp);
    }

    public float adjustAnglePencil = 90f;
    public float adjustAngleNeedle = -90f;


    private void RotateAnchorZLookAtBase()
    {
        if (anchorPencil != null)
        {
            Transform parent = anchorPencil.transform.parent;
            Vector3 localBasePos = parent != null
                ? parent.InverseTransformPoint(transform.position)
                : transform.position; // if no parent, world is local

            Vector3 localAnchorPos = anchorPencil.transform.localPosition;
            Vector3 dirPencilLocal = localBasePos - localAnchorPos;

            if (dirPencilLocal.sqrMagnitude > Mathf.Epsilon)
            {
                float angleDegreesPencil = (Mathf.Atan2(dirPencilLocal.y, dirPencilLocal.x) * Mathf.Rad2Deg) + adjustAnglePencil;
                anchorPencil.transform.localRotation = Quaternion.Euler(0f, 0f, angleDegreesPencil);
            }
        }

        if (anchorNeedle != null)
        {
            Transform parent = anchorNeedle.transform.parent;
            Vector3 localBasePos = parent != null
                ? parent.InverseTransformPoint(transform.position)
                : transform.position;

            Vector3 localAnchorPos = anchorNeedle.transform.localPosition;
            Vector3 dirNeedleLocal = localBasePos - localAnchorPos;

            if (dirNeedleLocal.sqrMagnitude > Mathf.Epsilon)
            {
                float angleDegreesNeedle = (Mathf.Atan2(dirNeedleLocal.y, dirNeedleLocal.x) * Mathf.Rad2Deg) + adjustAngleNeedle;
                anchorNeedle.transform.localRotation = Quaternion.Euler(0f, 0f, angleDegreesNeedle);
            }
        }
    }

    void ColliderFollowCenter()
    {
        if (rigCollider != null)
        {
            // Move the collider to the center of the rig by its X and z axis pos only
            Vector3 colliderPos = rigCollider.center;
            colliderPos.x = transform.localPosition.x;
            colliderPos.z = transform.localPosition.z;
            rigCollider.center = colliderPos;
        }
    }

    void OnDrawGizmos()
    {
        if (theAffectedA != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, theAffectedA.transform.position);
            Gizmos.DrawWireSphere(theAffectedA.transform.position, effectiveDistanceA);
        }

        if (theAffectedB != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, theAffectedB.transform.position);
            Gizmos.DrawWireSphere(theAffectedB.transform.position, effectiveDistanceB);
        }

        if (theAffectedA != null && theAffectedB != null && fixedDistanceTwoCircle > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(theAffectedA.transform.position, theAffectedB.transform.position);
        }
    }
}
