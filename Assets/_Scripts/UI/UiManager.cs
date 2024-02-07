using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class UiManager : MonoBehaviour
{

    [SerializeField] private PlayerData playerData;
    [SerializeField] private Match3Manager match3Manager;

    [Header("UI Info")]
    [SerializeField] private TMP_Text hammerCountText;
    [SerializeField] private TMP_Text colorBombCountText;
    [SerializeField] private TMP_Text rowDestroyCountText;
    [SerializeField] private TMP_Text shuffleCountText;

    [Header("Objective Info")]
    [SerializeField] private GameObject objectiveUiPrefab;
    [SerializeField] private RectTransform objectivesParent;
    [SerializeField] private Image durationFill;
    [SerializeField] private TMP_Text durationText;
    [SerializeField] private TMP_Text levelMovesText;
    [SerializeField] private TMP_Text playerScoresText;

    [Header("PopUps")]
    [SerializeField] private GameObject levelCompletedPopUp;
    [SerializeField] private GameObject levelFailedPopUp;
    [SerializeField] private TMP_Text levelCompletedScoresText;
    [SerializeField] private TMP_Text levelFailedScoresText;

    private bool isGameEnded;
    private Dictionary<string, TMP_Text> objectivesDict;

    public void OnLevelStart(GameLevel _level)
    {
        isGameEnded = false;
        playerScoresText.text = "0";
        objectivesDict = new Dictionary<string, TMP_Text>();

        if (_level.TimeLevel)
            StartCoroutine(StartTimer(_level.LevelDuration));

        if (_level.LevelMoves == Moves.Limited)
            levelMovesText.text = _level.MovesCount.ToString();

        if (_level.ShapesMatchTarget)
        {
            foreach (MatchShapeInfo _shapeInfo in _level.ShapesMatches)
            {
                AddNewObjective(_shapeInfo.Shape.ToString(), match3Manager.LevelsConfig.GetShapeInfo(_shapeInfo.Shape).shapeSprite, _shapeInfo.matches);
            }
        }

        if (match3Manager.ShellsRowAndColumn.Count > 0)
        {
            AddNewObjective(SpecialGridPiece.SHELL.ToString(), match3Manager.LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.SHELL).DisplaySprite, match3Manager.ShellsRowAndColumn.Count);
        }

        if (match3Manager.BlockRowAndColumn.Count > 0)
        {
            AddNewObjective(SpecialGridPiece.BLOCK.ToString(), match3Manager.LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.BLOCK).DisplaySprite, match3Manager.BlockRowAndColumn.Count);
        }

        objectivesParent.gameObject.SetActive(objectivesDict.Count > 0);
    }

    private void AddNewObjective(string _objective, Sprite _displaySprite, int _count)
    {
        var _objectiveUi = Instantiate(objectiveUiPrefab, objectivesParent);
        _objectiveUi.GetComponent<Image>().sprite = _displaySprite;
        objectivesDict[_objective] = _objectiveUi.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
        objectivesDict[_objective].text = _count.ToString();
    }

    public void OnScoresChanges(int _newScores)
    {
        playerScoresText.text = _newScores.ToString();
    }

    public void OnNumberOfMovesChange(int _movesLeft)
    {
        levelMovesText.text = _movesLeft.ToString();
        if (_movesLeft == 0)
            StartCoroutine(StartGameEnd(false));
    }

    public void OnShapesMatched(string _color, int _count)
    {
        if (objectivesDict.ContainsKey(_color))
        {
            objectivesDict[_color].text = _count.ToString();
        }
    }

    public void OnRemainingShellsCountChanged(int _newCount)
    {
        if (objectivesDict.ContainsKey(SpecialGridPiece.SHELL.ToString()))
        {
            objectivesDict[SpecialGridPiece.SHELL.ToString()].text = _newCount.ToString();
        }
    }

    public void OnRemainingBlocksCountChanged(int _newCount)
    {
        if (objectivesDict.ContainsKey(SpecialGridPiece.BLOCK.ToString()))
        {
            objectivesDict[SpecialGridPiece.BLOCK.ToString()].text = _newCount.ToString();
        }
    }

    public void OnObjectivesCompleted()
    {
        StartCoroutine(StartGameEnd(true));
    }

    public void PlayNextLevel()
    {
        playerData.CurrentLevel += 1;
        SceneManager.LoadScene("Gameplay");
    }

    public void OnGamePaused()
    {
        if (!isGameEnded)
            match3Manager.PauseLevel();
    }

    public void OnGameResumed()
    {
        if (!isGameEnded)
            match3Manager.ResumeLevel();
    }

    public void MoveToMenu() => LoadingUi.Instance.LoadScene("Menu");
    public void RetryLevel() => SceneManager.LoadScene("Gameplay");
    public void AddHammerPowerup(int count) => AddPowerUp(PowerUp.Hammer, count);
    public void AddRowColumnPowerup(int count) => AddPowerUp(PowerUp.RowColumn, count);
    public void AddColorBombPowerup(int count) => AddPowerUp(PowerUp.ColorBomb, count);
    public void AddShufflePowerup(int count) => AddPowerUp(PowerUp.Shuffle, count);
    public void ApplyHammer() => ApplyPowerUp(PowerUp.Hammer);
    public void ApplyRowColumn() => ApplyPowerUp(PowerUp.RowColumn);
    public void ApplyColorBomb() => ApplyPowerUp(PowerUp.ColorBomb);
    public void ApplyShuffle() => ApplyPowerUp(PowerUp.Shuffle);

    private void Start()
    {
        DisplayPlayerPowerupsCounts();
    }

    private IEnumerator StartTimer(float _time)
    {
        float currentTime = _time;
        durationFill.fillAmount = 1f;

        while (currentTime > 0)
        {
            currentTime -= Time.deltaTime;
            TimeSpan timeSpan = TimeSpan.FromSeconds(currentTime);
            if (timeSpan.TotalHours >= 1)
                durationText.text = string.Format("{0:00}:{1:00}:{2:00}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
            else if (timeSpan.TotalMinutes >= 1)
                durationText.text = string.Format("{0:00}:{1:00}", timeSpan.Minutes, timeSpan.Seconds);
            else
                durationText.text = string.Format("{0:00}", timeSpan.Seconds);

            durationFill.fillAmount = currentTime / _time;
            yield return null;
        }

        durationText.text = (currentTime < 0.5f) ? "00:00:00" : "00:00";
        StartCoroutine(StartGameEnd(false));
    }


    private void DisplayPlayerPowerupsCounts()
    {
        hammerCountText.text = playerData.HammerPowerUpCount.ToString();
        colorBombCountText.text = playerData.ColorBombPowerUpCount.ToString();
        rowDestroyCountText.text = playerData.RowDestroyPowerUpCount.ToString();
        shuffleCountText.text = playerData.ShufflePowerupCount.ToString();
    }

    private void AddPowerUp(PowerUp powerUp, int count)
    {
        playerData.AddPowerup(powerUp, count);
        DisplayPlayerPowerupsCounts();
    }

    private void ApplyPowerUp(PowerUp powerUp)
    {
        int powerUpCount = GetPowerUpCount(powerUp);
        if (powerUpCount == 0)
        {
            Debug.Log($"{powerUp} PowerUp Unavailable, Buy More From Shop");
            return;
        }

        if (!CanApplyPowerUp())
        {
            Debug.Log($"Cannot Apply {powerUp} Power Up");
            return;
        }

        DecreasePowerUpCount(powerUp);
        if (powerUp == PowerUp.Shuffle)
        {
            match3Manager.ShuffleShapes();
        }
        else
        {
            match3Manager.ApplyPowerUp(powerUp);
        }

        DisplayPlayerPowerupsCounts();
        Debug.Log($"{powerUp} Power Up Applied");
    }

    private int GetPowerUpCount(PowerUp powerUp)
    {
        switch (powerUp)
        {
            case PowerUp.Hammer:
                return playerData.HammerPowerUpCount;
            case PowerUp.RowColumn:
                return playerData.RowDestroyPowerUpCount;
            case PowerUp.ColorBomb:
                return playerData.ColorBombPowerUpCount;
            case PowerUp.Shuffle:
                return playerData.ShufflePowerupCount;
            default:
                return 0;
        }
    }

    private void DecreasePowerUpCount(PowerUp powerUp)
    {
        switch (powerUp)
        {
            case PowerUp.Hammer:
                playerData.HammerPowerUpCount -= 1;
                break;
            case PowerUp.RowColumn:
                playerData.RowDestroyPowerUpCount -= 1;
                break;
            case PowerUp.ColorBomb:
                playerData.ColorBombPowerUpCount -= 1;
                break;
            case PowerUp.Shuffle:
                playerData.ShufflePowerupCount -= 1;
                break;
        }
    }

    private bool CanApplyPowerUp()
    {
        return match3Manager.State == GameRunningState.None && match3Manager.CurrentPowerUp == PowerUp.None;
    }

    private IEnumerator StartGameEnd(bool _levelCompleted)
    {
        isGameEnded = true;
        match3Manager.EndLevel();
        yield return new WaitUntil(() => match3Manager.State == GameRunningState.None);
        yield return new WaitForSeconds(1f);
        if (_levelCompleted)
        {
            levelCompletedScoresText.text = playerScoresText.text;
            levelCompletedPopUp.SetActive(true);
        }
        else
        {
            levelFailedScoresText.text = playerScoresText.text;
            levelFailedPopUp.SetActive(true);
        }
    }

}