using System.Collections.Generic;
using UnityEngine;

public class DraggableIconHandler : StaticReference<DraggableIconHandler>
{
    [Header("Prefab")]
    [SerializeField] private UIDraggableIcon uiPrefab;
    [SerializeField] private int totalSpawn;

    [Header("References")]
    [SerializeField] private GameObject parentToSpawn;

    [Header("Data")]
    public List<UIDraggableIcon> spawnedIcons = new();


    void Awake()
    {
        BaseAwake(this);
    }

    void OnDestroy()
    {
        BaseOnDestroy();
    }

    // call this in GameplayController to spawn the icons
    public void SpawnIcons(int totalSpawn)
    {
        for (int i = 0; i < totalSpawn; i++)
        {
            if (uiPrefab == null)
            {
                Debug.LogError("UI Prefab is not assigned.");
                return;
            }

            UIDraggableIcon iconInstance = Instantiate(uiPrefab, parentToSpawn.transform);
            iconInstance.transform.SetParent(parentToSpawn.transform, false);

            spawnedIcons.Add(iconInstance);
        }
    }


}
