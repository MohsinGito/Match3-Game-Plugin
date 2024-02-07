using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Match3ObjectivesTracker : MonoBehaviour
{

    private GameLevel currentLevel;
    private ObjectivesInfo objectivesInfo;
    private Match3Manager match3Manager;
    private LevelsConfig levelsConfig;
    private Match3Events events;

    public void Init(Match3Manager _match3Manager, LevelsConfig _levelConfig, Match3Events _events)
    {
        events = _events;
        match3Manager = _match3Manager;
        levelsConfig = _levelConfig;

        events.LevelEvents.OnLevelStart.AddListener(OnLevelStart);
        events.LevelEvents.OnShapesMatched.AddListener(OnShapesMatched);
        events.LevelEvents.OnSuccessfullMove.AddListener(OnSuccessfullMove);
        events.LevelEvents.OnRocksDestroyed.AddListener(OnRocksDestroyed);
        events.LevelEvents.OnShellsDestroyed.AddListener(OnShellsDestroyed);
    }

    public void ClearEvents()
    {
        events.LevelEvents.OnLevelStart.RemoveListener(OnLevelStart);
        events.LevelEvents.OnShapesMatched.RemoveListener(OnShapesMatched);
        events.LevelEvents.OnSuccessfullMove.RemoveListener(OnSuccessfullMove);
        events.LevelEvents.OnRocksDestroyed.RemoveListener(OnRocksDestroyed);
        events.LevelEvents.OnShellsDestroyed.RemoveListener(OnShellsDestroyed);
    }

    private void OnLevelStart(GameLevel _level)
    {
        if (_level == null)
        {
            Debug.LogError("OnLevelStart received a null GameLevel.");
            return;
        }

        currentLevel = _level;
        SetUpObjectives();
    }

    private void OnShapesMatched(string _shape, int _count)
    {
        if (!IsLevelInitialized())
            return;

        AddScores(_count * levelsConfig.ScoreForASinglePiece);

        if (!currentLevel.ShapesMatchTarget)
            return;

        ShapeType _shapeType = StringToShape(_shape);
        if (!objectivesInfo.ShapesMatches.ContainsKey(_shapeType))
            return;

        if (objectivesInfo.ShapesMatches[_shapeType] != 0)
        {
            objectivesInfo.ShapesMatches[_shapeType] -= _count;
            objectivesInfo.ShapesMatches[_shapeType] = objectivesInfo.ShapesMatches[_shapeType] < 0 ? 0 : objectivesInfo.ShapesMatches[_shapeType];
            events.ObjectivesEvents.OnShapeColorMatchEvent?.Invoke(_shape, objectivesInfo.ShapesMatches[_shapeType]);
        }

        if (AllObjectivesAchieved())
            events.ObjectivesEvents?.OnAllObjectivesCleared?.Invoke();
    }

    private void OnSuccessfullMove()
    {
        if (!IsLevelInitialized())
            return;

        if (currentLevel.LevelMoves == Moves.UnLimited)
            return;

        objectivesInfo.NumberOfMoves -= 1;
        events.ObjectivesEvents.OnNumberOfMovesLeftChanged?.Invoke(objectivesInfo.NumberOfMoves);
    }

    private void OnRocksDestroyed(int rocksCount)
    {
        if (!IsLevelInitialized())
            return;

        objectivesInfo.RockBlocksCount -= rocksCount;
        objectivesInfo.RockBlocksCount = objectivesInfo.RockBlocksCount <= 0 ? 0 : objectivesInfo.RockBlocksCount;
        events.ObjectivesEvents.OnRemainingBlocksCountChanged?.Invoke(objectivesInfo.RockBlocksCount);
        AddScores(rocksCount * levelsConfig.ScoreForASpecialPiece);

        if (AllObjectivesAchieved())
            events.ObjectivesEvents?.OnAllObjectivesCleared?.Invoke();
    }

    private void OnShellsDestroyed(int shellsCount)
    {
        if (!IsLevelInitialized())
            return;
         
        objectivesInfo.ShellBlocksCount -= shellsCount;
        objectivesInfo.ShellBlocksCount = objectivesInfo.ShellBlocksCount <= 0 ? 0 : objectivesInfo.ShellBlocksCount;
        events.ObjectivesEvents.OnRemainingShellsCountChanged?.Invoke(objectivesInfo.ShellBlocksCount);
        AddScores(shellsCount * levelsConfig.ScoreForASpecialPiece);

        if (AllObjectivesAchieved())
            events.ObjectivesEvents?.OnAllObjectivesCleared?.Invoke();
    }

    private void SetUpObjectives()
    {
        if (match3Manager == null)
        {
            Debug.LogError("SetUpObjectives: Match3Manager not initialized.");
            return;
        }

        if (currentLevel.TargetScores || currentLevel.LevelMoves == Moves.Limited || match3Manager.ShellsRowAndColumn.Count > 0 || match3Manager.BlockRowAndColumn.Count > 0 || currentLevel.ShapesMatchTarget)
            objectivesInfo = new ObjectivesInfo();

        if (currentLevel.TargetScores)
            objectivesInfo.Scores = 0;

        if (currentLevel.LevelMoves == Moves.Limited)
            objectivesInfo.NumberOfMoves = currentLevel.MovesCount;

        if (match3Manager.ShellsRowAndColumn.Count > 0)
            objectivesInfo.ShellBlocksCount = match3Manager.ShellsRowAndColumn.Count;

        if (match3Manager.BlockRowAndColumn.Count > 0)
            objectivesInfo.RockBlocksCount = match3Manager.BlockRowAndColumn.Count;

        if (currentLevel.ShapesMatchTarget)
        {
            objectivesInfo.ShapesMatches = new Dictionary<ShapeType, int>();
            foreach (MatchShapeInfo _shapeInfo in currentLevel.ShapesMatches)
                objectivesInfo.ShapesMatches[_shapeInfo.Shape] = _shapeInfo.matches;
        }
    }

    private bool AllObjectivesAchieved()
    {
        if (objectivesInfo.ShellBlocksCount > 0)
        {
            if (objectivesInfo.ShellBlocksCount == 0)
                Debug.Log("All Jelies Cleared");
            else
                return false;
        }

        if (objectivesInfo.RockBlocksCount > 0)
        {
            if (objectivesInfo.RockBlocksCount == 0)
                Debug.Log("All Rocks Cleared");
            else
                return false;
        }

        if (currentLevel.ShapesMatchTarget)
        {
            foreach (KeyValuePair<ShapeType, int> _shapeInfo in objectivesInfo.ShapesMatches)
            {
                if (_shapeInfo.Value == 0)
                    continue;

                return false;
            }
        }

        ClearEvents();
        return true;
    }

    public void AddScores(int _newScores)
    {
        objectivesInfo.Scores += _newScores;
        events.ObjectivesEvents.OnScoresChanged?.Invoke(objectivesInfo.Scores);
    }

    private bool IsLevelInitialized()
    {
        if (objectivesInfo == null)
        {
            return false;
        }

        if (currentLevel == null)
        {
            return false;
        }

        return true;
    }

    private List<ShapeType> StringToTypeList(List<string> _shapes)
    {
        return _shapes.Select(n => StringToShape(n)).ToList();
    }

    public ShapeType StringToShape(string _shape)
    {
        if (Enum.TryParse<ShapeType>(_shape, out ShapeType result))
            return result;
        else
            return default(ShapeType);
    }

}