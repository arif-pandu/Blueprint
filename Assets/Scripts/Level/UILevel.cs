using TMPro;
using UnityEngine;

public class UILevel : MonoBehaviour
{
    [SerializeField] private TMP_Text textLevel;

    private void Awake()
    {
        if (textLevel == null)
        {
            Debug.LogError("TextLevel is not assigned in UILevel.");
        }
    }

    private void OnEnable()
    {
        // Load the highest level ID from PlayerPrefs
        int highestLevelID = PlayerPrefs.GetInt("HighestLevelID", 0);

        // Set the text to display the current level
        if (textLevel != null)
        {
            textLevel.text = "Level: " + (highestLevelID + 1).ToString();
        }
        else
        {
            Debug.LogError("TextLevel is not assigned in UILevel.");
        }
    }
}
