using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;


    [Header("Materials")]
    [SerializeField] private Color activeColor;
    [SerializeField] private Color inactiveColor;

    [SerializeField] private float colorTransitionDuration = 0.5f;
    [SerializeField] private LeanTweenType colorTransitionEase = LeanTweenType.easeInOutQuad;

    [Header("State")]

    private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            if (isActive != value)
            {
                SetMaterialByState(value);
            }
        }
    }


    void Start()
    {
        SetMaterialByState(isActive);
    }




    public void SetMaterialByState(bool isActive)
    {
        if (spriteRenderer == null)
        {
            Debug.LogWarning("MeshRenderer is not assigned.");
            return;
        }

        Debug.Log($"Setting target state to {(isActive ? "active" : "inactive")}");
        
        this.isActive = isActive;
        

        // animate color using LeanTween
        Color targetColor = isActive ? activeColor : inactiveColor;
        LeanTween.cancel(spriteRenderer.gameObject); // Cancel any ongoing color transitions
        LeanTween.color(spriteRenderer.gameObject, targetColor, colorTransitionDuration)
            .setEase(colorTransitionEase);
            // .setOnComplete(() => this.isActive = isActive); // Update state after transition
    }
}
