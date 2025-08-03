using UnityEngine;

public class GameplayController : StaticReference<GameplayController>
{
    [Header("References")]
    [SerializeField] private TargetHandler targetHandler;
    [SerializeField] private DraggableIconHandler draggableIconHandler;
    [SerializeField] private CompassHandler compassHandler;
    [SerializeField] private HelpHandler helpHandler;
    [SerializeField] private WinningHandler winningHandler;
    [SerializeField] private LevelGlossary levelGlossary;

    [Header("Level Data")]
    private Level levelData;



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
        // Load the highest level data
        LoadHighestLevelData();

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

    private void LoadHighestLevelData()
    {
        // Load the highest level data from LevelGlossary
        LevelGlossary levelGlossary = FindObjectOfType<LevelGlossary>();
        if (levelGlossary != null)
        {
            int highestLevelID = PlayerPrefs.GetInt("HighestLevelID", 0); // Default to level 1 if not set

            LevelData data = levelGlossary.GetLevelData(highestLevelID);
            levelData = data.Level;
            if (levelData == null)
            {
                Debug.LogError("Failed to load level data.");
            }
        }
        else
        {
            Debug.LogError("LevelGlossary not found in the scene.");
        }
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
        winningHandler.OnSetupWinPanel();
    }


}
