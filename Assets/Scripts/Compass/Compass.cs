using UnityEngine;

public class Compass : MonoBehaviour
{
    [Header("Animation")]
    public LeanTweenType spawnEase = LeanTweenType.easeInOutSine;
    public float durationSpawnAnim = 0.125f;


    [Header("Setup")]
    public LayerMask raycastLayers;
    public bool preserveY = true;


    [Header("State")]
    private Camera cam;
    private int currentTweenId = -1;


    public void AnimateSpawn()
    {
        LeanTween.cancel(currentTweenId);
        currentTweenId = LeanTween.scale(gameObject, Vector3.one, durationSpawnAnim)
            .setEase(spawnEase)
            .setOnComplete(() => currentTweenId = -1)
            .id;
    }





}
