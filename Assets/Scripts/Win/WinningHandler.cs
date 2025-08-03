using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinningHandler : MonoBehaviour
{
    [SerializeField] private GameObject holder;

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


    }
}
