using UnityEngine;
using UnityEngine.UI;

public class HelpHandler : MonoBehaviour
{
    [SerializeField] private GameObject anchor;
    [SerializeField] private GameObject movingHandler;

    [Header("Angle Sweep")]
    [SerializeField] private float startAngle = 0f; // degrees
    [SerializeField] private float endAngle = 90f;  // degrees
    [SerializeField] private bool clockwise = false; // if true, start->end travels clockwise
    [SerializeField] private float rotationSpeed = 30f; // degrees per second

    [Header("Target Visuals")]
    [SerializeField] private Image imageTarget;
    [SerializeField] private Sprite targetActiveSprite;
    [SerializeField] private Sprite targetInactiveSprite;

    [Header("Text Hints")]
    [SerializeField] private Image imageText;
    [SerializeField] private Sprite textRotateSprite;
    [SerializeField] private Sprite textConnectSprite;

    [Header("Arc Progress")]
    [SerializeField] private Image arcImage; // expected to be a Filled Image

    // Internal
    private RectTransform anchorRect;
    private RectTransform handlerRect;
    private float currentAngle;
    private int phase = 0; // 0 = start->end, 1 = end->start
    private float radius;
    private bool initialized = false;
    private float phaseFromAngle;
    private float phaseToAngle;
    private float fullPhaseDelta; // magnitude along preferred direction

    // Threshold logging
    private bool hasLoggedThresholdThisPhase = false;
    private const float logThreshold = 0.45f;
    private const float minFill = 0.1f;
    private const float maxFill = 0.69f;

    void Start()
    {
        if (anchor != null)
            anchorRect = anchor.GetComponent<RectTransform>();
        if (movingHandler != null)
            handlerRect = movingHandler.GetComponent<RectTransform>();

        if (anchorRect == null || handlerRect == null)
            return;

        radius = Vector2.Distance(handlerRect.anchoredPosition, anchorRect.anchoredPosition);
        currentAngle = NormalizeAngle(startAngle);
        phase = 0;
        SetupPhaseEndpoints();

        ApplyAngle(currentAngle);
        UpdateArcFill();

        if (imageText != null && textRotateSprite != null)
            imageText.sprite = textRotateSprite;

        initialized = true;
    }

    void Update()
    {
        if (!initialized || anchorRect == null || handlerRect == null)
            return;

        bool preferClockwise = (phase == 0) ? clockwise : !clockwise;
        float targetAngle = phaseToAngle;

        float deltaAngle = DeltaAngleWithPreference(currentAngle, targetAngle, preferClockwise);
        float step = rotationSpeed * Time.deltaTime;

        bool reached = false;
        if (Mathf.Abs(deltaAngle) <= step)
        {
            currentAngle = targetAngle;
            reached = true;
        }
        else
        {
            currentAngle = NormalizeAngle(currentAngle + Mathf.Sign(deltaAngle) * step);
        }

        ApplyAngle(currentAngle);
        UpdateArcFill();

        if (reached)
        {
            phase = 1 - phase;
            SetupPhaseEndpoints();
            hasLoggedThresholdThisPhase = false; // reset logging flag on phase change

            if (imageText != null)
            {
                if (phase == 0 && textRotateSprite != null)
                    imageText.sprite = textRotateSprite;
                else if (phase == 1 && textConnectSprite != null)
                    imageText.sprite = textConnectSprite;
            }
        }
    }

    private void SetupPhaseEndpoints()
    {
        if (phase == 0)
        {
            phaseFromAngle = NormalizeAngle(startAngle);
            phaseToAngle = NormalizeAngle(endAngle);
        }
        else
        {
            phaseFromAngle = NormalizeAngle(endAngle);
            phaseToAngle = NormalizeAngle(startAngle);
        }

        bool preferClockwise = (phase == 0) ? clockwise : !clockwise;
        fullPhaseDelta = Mathf.Abs(DeltaAngleWithPreference(phaseFromAngle, phaseToAngle, preferClockwise));
        if (fullPhaseDelta < Mathf.Epsilon)
            fullPhaseDelta = 1f;
    }

    /// <summary>
    /// Updates arcImage.fillAmount so that during start->end it increases 0→1 (remapped to [0.1,0.7]),
    /// and during end->start it decreases 1→0, with a one-time debug log when exceeding 0.45.
    /// </summary>
    private void UpdateArcFill()
    {
        if (arcImage == null) return;

        bool preferClockwise = (phase == 0) ? clockwise : !clockwise;
        float traveled = Mathf.Abs(DeltaAngleWithPreference(phaseFromAngle, currentAngle, preferClockwise));
        float t = Mathf.Clamp01(traveled / fullPhaseDelta); // normalized 0..1

        float fill;
        if (phase == 0)
        {
            fill = Mathf.Lerp(minFill, maxFill, t);
        }
        else
        {
            fill = Mathf.Lerp(maxFill, minFill, t);
        }

        fill = Mathf.Clamp(fill, minFill, maxFill);
        arcImage.fillAmount = fill;

        if (!hasLoggedThresholdThisPhase && fill > logThreshold)
        {
            Debug.Log($"Arc fill crossed threshold {logThreshold:F2} on phase {phase}: fill={fill:F3}");
            hasLoggedThresholdThisPhase = true;
            ChangeImageTarget(true); // set target active when crossing threshold
        }
        else if (hasLoggedThresholdThisPhase && fill < logThreshold)
        {
            Debug.Log($"Arc fill dropped below threshold {logThreshold:F2} on phase {phase}: fill={fill:F3}");
            hasLoggedThresholdThisPhase = false; // reset for next phase
            ChangeImageTarget(false); // set target inactive when dropping below threshold
        }
    }

    private void ChangeImageTarget(bool isActive)
    {
        if (imageTarget == null) return;
        imageTarget.sprite = isActive ? targetActiveSprite : targetInactiveSprite;
        imageTarget.SetNativeSize(); // Adjust size to fit the new sprite        
    }

    private float DeltaAngleWithPreference(float current, float target, bool preferClockwise)
    {
        current = NormalizeAngle(current);
        target = NormalizeAngle(target);
        float rawDelta = Mathf.DeltaAngle(current, target);

        if (preferClockwise)
        {
            if (rawDelta < 0)
                return rawDelta;
            return rawDelta - 360f;
        }
        else
        {
            if (rawDelta > 0)
                return rawDelta;
            return rawDelta + 360f;
        }
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0) a += 360f;
        return a;
    }

    private void ApplyAngle(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector2 offset = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * radius;
        handlerRect.anchoredPosition = anchorRect.anchoredPosition + offset;
    }

    public void SetTargetActive(bool active)
    {
        if (imageTarget == null) return;
        imageTarget.sprite = active ? targetActiveSprite : targetInactiveSprite;
    }

    public void ShowRotateHint()
    {
        if (imageText != null && textRotateSprite != null)
            imageText.sprite = textRotateSprite;
    }

    public void ShowConnectHint()
    {
        if (imageText != null && textConnectSprite != null)
            imageText.sprite = textConnectSprite;
    }
}
