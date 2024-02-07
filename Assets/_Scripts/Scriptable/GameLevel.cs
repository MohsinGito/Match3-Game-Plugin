using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Create Level")]
public class GameLevel : ScriptableObject
{
    public int Rows;
    public int Columns;
    public Grid2DLayout LevelGrid;
    public Vector2 CellSize = new Vector2(1, 1);
    public Vector2 ScreenOffset = new Vector2(1, 1);

    [Range(0f, 2f)]
    public float ShapeSizeFactor = 1.0f;

    public bool RandomizeColorShapes;
    public GridShapes GridShapes;

    [Header("Shape Pieces Info")]
    public List<ShapeType> LevelShapes;
    public int BoardBombChances = 0;
    public bool CanAddBonus = true;
    public bool CanShowHint = true;

    [Header("Level Controlls")]
    public Moves LevelMoves;
    public int MovesCount;
    public bool TimeLevel;
    public float LevelDuration;

    [Header("Level Objectives")]
    public bool ContainsJellyBlocks;
    public Grid2DLayout JellyRowAndColumn;
    public bool ContainsRockBlocks;
    public Grid2DLayout RockRowAndColumns;
    public bool ShapesMatchTarget;
    public List<MatchShapeInfo> ShapesMatches;
    public bool TargetScores;
    public int ScoreTarget;

    [Header("Level Completion Info")]
    public bool LevelCompleted = false;
    public int ScoresEarned = 0;

    public void LevelOver(int _newScore)
    {
        if (_newScore > ScoresEarned)
        {
            ScoresEarned = _newScore;
            LevelCompleted = StarsAwarded() != 0 ? true : false;
        }
    }

    public int StarsAwarded()
    {
        if (ScoresEarned >= ScoreTarget)
            return 3;

        if (ScoresEarned >= ScoreTarget / 2)
            return 2;

        if (ScoresEarned >= ScoreTarget / 3)
            return 1;

        return 0;
    }

    public void PopulateGridShapes()
    {
        GridShapes.gridRows = new List<GridShapeRow>();

        for (int i = 0; i < LevelGrid.rows.Length; i++)
        {
            GridShapeRow newRow = new GridShapeRow();
            newRow.shapesRow = new List<ShapeTypeEditor>();

            for (int j = 0; j < LevelGrid.rows[i].row.Length; j++)
            { 
                newRow.shapesRow.Add(ShapeTypeEditor.EMPTY);
            }

            GridShapes.gridRows.Add(newRow);
        }
    }

}

[System.Serializable]
public struct MatchShapeInfo
{
    public ShapeType Shape;
    public int matches;
}

public enum Moves
{
    UnLimited,
    Limited
}

[System.Serializable]
public struct GridShapes
{
    public List<GridShapeRow> gridRows;
}

[System.Serializable]
public struct GridShapeRow
{
    public List<ShapeTypeEditor> shapesRow;
}

public enum ShapeTypeEditor
{
    BLUE,
    GREEN,
    PURPLE,
    RED,
    YELLOW,
    BLOCK,
    SHELL,
    EMPTY,
    BOARD_BOMB
}