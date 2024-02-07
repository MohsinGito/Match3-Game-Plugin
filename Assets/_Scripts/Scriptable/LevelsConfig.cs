using System;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Levels Config", menuName = "Levels Config")]
public class LevelsConfig : ScriptableObject
{

    [Header("Shpes Info")]
    public List<Match3ShapeInfo> shapes;
    public List<SpecialPieceInfo> SpecialPieces;
    public GameObject ShapeBg;
    public GameObject ShapePrefab;

    [Header("Shapes Base Info")]
    public int ScoreForASinglePiece;
    public int ScoreForASpecialPiece;
    public float CollapseSpeedFactor;
    public float WaitBeforeCollapsing;
    public float WaitForCollapsing;
    public float WaitAfterCollapsing;
    public float WaitForPotentialMatch;
    public float WaitBeforeActivatingColorBomb;

    [Header("Matches Info")]
    public int MinimumMatches = 3;
    public int MinimumMatchesForBonus = 4;
    public int MinimumMatchesForColorBomb = 5;

    [Header("Shapes Movement")]
    public float ShapesFallHeight;
    public float ShapesSwapDuration;
    public float ShapesCollapseDuration;

    [Header("Ascending Levels List")]
    public List<GameLevel> GameLevels;

    public GameLevel GetLevel(int _level)
    {
        if (_level >= GameLevels.Count)
            return GameLevels[0];

        return GameLevels[_level];
    }

    public Match3ShapeInfo GetShapeInfo(ShapeType _type)
    {
        return shapes.Find(n => n.type == _type);
    }

    public SpecialPieceInfo GetSpecialPieceInfo(SpecialGridPiece _type)
    {
        return SpecialPieces.Find(n => n.Type == _type);
    }

}

[Serializable]
public struct SpecialPieceInfo
{
    public SpecialGridPiece Type;
    public Sprite DisplaySprite;
}

[Serializable]
public struct Match3ShapeInfo
{
    public ShapeType type;
    public Sprite shapeSprite;
    public Sprite bonusSprite;
}

public enum ShapeType
{
    BLUE,
    GREEN,
    PURPLE,
    RED,
    YELLOW
}

public enum SpecialGridPiece
{
    COLOR_BOMB,
    BLOCK,
    SHELL,
    BOARD_BOMB,
    NONE
}