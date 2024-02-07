using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LevelMap : MonoBehaviour
{

    [SerializeField] private LevelsPatch levelMapPatch;
    [SerializeField] private RectTransform patchesParent;
    [SerializeField] private PlayerData playerData;
    [SerializeField] private LevelsConfig levels;

    private List<LevelUi> mapLevels;
    private List<LevelsPatch> mapPatches;

    private void Start()
    {
        SetupLevelsMap();
    }

    public void Display()
    {

        int _levelUiIndex = 0;
        foreach (GameLevel _level in levels.GameLevels)
        {
            if (_level.LevelCompleted || _levelUiIndex == 0)
            {
                mapLevels[_levelUiIndex].Enable(_level.StarsAwarded(), OnLevelSelected);
            }
            else
            {
                if (levels.GameLevels[_levelUiIndex - 1].LevelCompleted)
                {
                    mapLevels[_levelUiIndex].Enable(_level.StarsAwarded(), OnLevelSelected);
                    break;
                }
            }

            _levelUiIndex += 1;
        }
        StartCoroutine(SetFocusOnActiveLevel(0));
    }

    public void UnLockAllLevels()
    {
        for (int i = 0; i < mapLevels.Count; i++)
            mapLevels[i].Enable(levels.GameLevels[i].StarsAwarded(), OnLevelSelected);
    }

    public void SetupLevelsMap()
    {
        mapLevels = new List<LevelUi>();
        mapPatches = new List<LevelsPatch>();

        for (int i = 0; i < levels.GameLevels.Count; i++)
        {
            if ((i) % 11 == 0 || i == 0)
            {
                AddNewMapPatch();
                mapPatches[mapPatches.Count - 1].Init();
            }

            mapLevels.Add(mapPatches[mapPatches.Count - 1].SetUpNewLevel());
            mapLevels[mapLevels.Count - 1].Disable(i);
        }

        if (mapLevels.Count != levels.GameLevels.Count)
        {
            Debug.LogError("Error While Setting Up All Levels");
            return;
        }
    }

    private void OnLevelSelected(int _levelIndex)
    {
        DOTween.KillAll();
        playerData.CurrentLevel = _levelIndex;
        SceneManager.LoadScene("Gameplay");
    }

    private IEnumerator SetFocusOnActiveLevel(int _levelIndex)
    {
        yield return new WaitForEndOfFrame();

        if (mapPatches.Count > 0)
        {
            int patchToFocus = Mathf.CeilToInt(_levelIndex / 11);

            if (patchToFocus < mapPatches.Count)
            {
                float contentHeight = patchesParent.sizeDelta.y;
                float targetHeight = contentHeight / (mapPatches.Count - 1) * patchToFocus;
                float scrollPosition = targetHeight / patchesParent.sizeDelta.y;
                yield return StartCoroutine(MoveScrollRect(patchesParent.parent.GetComponent<ScrollRect>(), scrollPosition));
            }
        }
    }

    private IEnumerator MoveScrollRect(ScrollRect _scrollRect, float _targetPos)
    {
        float _duration = 0.5f;
        float _timeElapsed = 0f;
        float _startPos = _scrollRect.verticalNormalizedPosition;

        while (_timeElapsed < _duration)
        {
            _scrollRect.verticalNormalizedPosition = Mathf.Lerp(_startPos, _targetPos, _timeElapsed / _duration);
            _timeElapsed += Time.deltaTime;
            yield return null;
        }

        _scrollRect.verticalNormalizedPosition = _targetPos;
    }


    private void AddNewMapPatch()
    {
        LevelsPatch newPatch = Instantiate(levelMapPatch, patchesParent);
        newPatch.transform.SetAsFirstSibling();
        mapPatches.Add(newPatch);
    }

}