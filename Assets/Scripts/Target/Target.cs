using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer meshRenderer;


    [Header("Materials")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    [Header("State")]

    private bool isActive = false;


    void Start()
    {
        SetMaterialByState(isActive);
    }




    public void SetMaterialByState(bool isActive)
    {
        if (meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer is not assigned.");
            return;
        }

        meshRenderer.material = isActive ? activeMaterial : inactiveMaterial;
    }
}
