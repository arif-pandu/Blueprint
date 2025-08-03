using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningHandler : MonoBehaviour
{
    [SerializeField] private GameObject holder;
    [SerializeField] private GameObject holderMaxLevel;

    private int currentLevel;
    private int maxLevel = 9; // Set this to the maximum level you want to allow

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("HighestLevelID", 0);
    }

    public void OnSetupWinPanel()
    {
        if (currentLevel >= maxLevel)
        {
            // If the current level is the maximum level, show the max level holder
            holder.SetActive(false);
            holderMaxLevel.SetActive(true);
        }
        else
        {
            // Otherwise, show the regular win holder
            holder.SetActive(true);
            holderMaxLevel.SetActive(false);
        }
    }
    public void OnTapHome()
    {
        SceneManager.LoadScene("MainMenuScene");

    }

    public void OnTapNext()
    {
        // Increment the current level and save it
        currentLevel++;
        PlayerPrefs.SetInt("HighestLevelID", currentLevel);
        PlayerPrefs.Save();

        // ReLoad current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
