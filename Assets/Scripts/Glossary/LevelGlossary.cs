using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelGlossary : MonoBehaviour
{
    [SerializeField] private List<LevelData> levels = new();

    public LevelData GetLevelData(int levelID)
    {
        foreach (var levelData in levels)
        {
            if (levelData.LevelID == levelID)
            {
                return levelData;
            }
        }
        Debug.LogError($"Level with ID {levelID} not found.");
        return null;
    }
}

[Serializable]
public class LevelData
{
    public int LevelID;
    public Level Level;
}
