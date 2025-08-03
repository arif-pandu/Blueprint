using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningHandler : MonoBehaviour
{
    [SerializeField] private GameObject holder;

    private int currentLevel;

    void Start()
    {
        currentLevel = PlayerPrefs.GetInt("HighestLevelID", 0);
    }

    public void OnSetupWinPanel()
    {
        if (holder == null)
        {
            Debug.LogError("Holder is not assigned in WinningHandler.");
            return;
        }

        holder.SetActive(true);
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
