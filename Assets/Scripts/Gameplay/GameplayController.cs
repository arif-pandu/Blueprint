using UnityEngine;

public class GameplayController : StaticReference<GameplayController>
{
    [Header("References")]
    [SerializeField] private TargetHandler targetHandler;
    [SerializeField] private DraggableIconHandler draggableIconHandler;
    [SerializeField] private CompassHandler compassHandler;
    [SerializeField] private HelpHandler helpHandler;

    [Header("Level Data")]
    public Level levelData;



    private void Awake()
    {
        BaseAwake(this);
    }

    private void OnDestroy()
    {
        BaseOnDestroy();
    }

    private void Start()
    {
        if (targetHandler == null)
        {
            Debug.LogError("TargetHandler is not assigned in GameplayController.");
            return;
        }
        if (draggableIconHandler == null)
        {
            Debug.LogError("DraggableIconHandler is not assigned in GameplayController.");
            return;
        }
        if (compassHandler == null)
        {
            Debug.LogError("CompassHandler is not assigned in GameplayController.");
            return;
        }
        if (helpHandler == null)
        {
            Debug.LogError("HelpHandler is not assigned in GameplayController.");
            return;
        }


        SetupGameplay();

    }


    private void SetupGameplay()
    {
        // Spawn draggable icons
        draggableIconHandler.SpawnIcons(levelData.AvailableCompasses);

        // Spawn targets
        targetHandler.SpawnTargets(levelData);

    }


    public void SetupGameFinished()
    {
        Debug.Log("Level Completed");
    }


}
