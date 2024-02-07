using UnityEngine;
using System.Collections.Generic;

public class LevelsPatch : MonoBehaviour
{

    [SerializeField] private List<LevelUi> levels;

    private int currentActiveLevel = 0;

    public void Init()
    {
        foreach (LevelUi levelUi in levels)
            levelUi.gameObject.SetActive(false);
    }

    public LevelUi SetUpNewLevel()
    {
        if (currentActiveLevel == levels.Count)
            return null;


        LevelUi nextLevel = levels[currentActiveLevel];
        nextLevel.gameObject.SetActive(true);
        currentActiveLevel += 1;
        return nextLevel;
    }

}