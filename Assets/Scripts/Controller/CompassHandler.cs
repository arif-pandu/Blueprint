using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompassHandler : StaticReference<CompassHandler>
{
    [Header("Prefab")]
    [SerializeField] private Compass compassPrefab;


    [Header("References")]
    [SerializeField] private GameObject parentToSpawn;

    [Header("Data")]
    public List<Compass> spawnedCompasses = new();



    void Awake()
    {
        BaseAwake(this);
    }
    void OnDestroy()
    {
        BaseOnDestroy();
    }


    public void SpawnCompassAtWorldPos(Vector3 pos)
    {
        if (compassPrefab == null)
        {
            Debug.LogError("Compass prefab is not assigned.");
            return;
        }

        Compass compassInstance = Instantiate(compassPrefab, pos, Quaternion.identity);
        compassInstance.transform.SetParent(parentToSpawn.transform, false);
        // compassInstance.AnimateSpawn();

        spawnedCompasses.Add(compassInstance);

    }
}
