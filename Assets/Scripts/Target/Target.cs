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


    private bool isActive = false;


    void Start()
    {
        SetMaterialByState(isActive);
    }




    void SetMaterialByState(bool isActive)
    {
        if (meshRenderer == null)
        {
            Debug.LogWarning("MeshRenderer is not assigned.");
            return;
        }

        meshRenderer.material = isActive ? activeMaterial : inactiveMaterial;
    }
}
