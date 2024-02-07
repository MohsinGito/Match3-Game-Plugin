using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

#region CLASSES

public class MatchesInfo
{
    private List<GameObject> matchedShapes;

    public IEnumerable<GameObject> MatchedShape
    {
        get
        {
            return matchedShapes.Distinct();
        }
    }

    public void AddObject(GameObject go)
    {
        if (go == null)
            return;

        if (!matchedShapes.Contains(go))
            matchedShapes.Add(go);
    }

    public void AddObjectRange(IEnumerable<GameObject> gos)
    {
        foreach (var item in gos)
        {
            AddObject(item);
        }
    }

    public MatchesInfo()
    {
        matchedShapes = new List<GameObject>();
        BonusesContained = BonusType.None;
    }

    public BonusType BonusesContained { get; set; }
}

public class ModShapeInfo
{
    private List<GameObject> newShape { get; set; }
    public int MaxDistance { get; set; }

    public IEnumerable<GameObject> ModShape
    {
        get
        {
            return newShape.Distinct();
        }
    }

    public void AddShape(GameObject go)
    {
        if (!newShape.Contains(go))
            newShape.Add(go);
    }

    public ModShapeInfo()
    {
        newShape = new List<GameObject>();
    }
}

public class GameObjectShapeComparer : IEqualityComparer<GameObject>
{
    public bool Equals(GameObject x, GameObject y)
    {
        var shapeX = x.GetComponent<Shape>();
        var shapeY = y.GetComponent<Shape>();

        if (shapeX == null || shapeY == null)
            return false;

        return shapeX.Row == shapeY.Row && shapeX.Column == shapeY.Column;
    }

    public int GetHashCode(GameObject obj)
    {
        var shape = obj.GetComponent<Shape>();
        if (shape == null)
            return 0;

        int hash = 13;
        hash = (hash * 7) + shape.Row.GetHashCode();
        hash = (hash * 7) + shape.Column.GetHashCode();
        return hash;
    }
}

public class ShapeInfo
{
    public int Column { get; set; }
    public int Row { get; set; }
}

public static class BonusTypeUtilities
{
    public static bool ContainsDestroyWholeRowColumn(BonusType bt)
    {
        return (bt & BonusType.DestroyWholeRowColumn)
            == BonusType.DestroyWholeRowColumn;
    }
}

public class ObjectivesInfo
{
    public int Scores;
    public int NumberOfMoves;
    public int ShellBlocksCount;
    public int RockBlocksCount;
    public Dictionary<ShapeType, int> ShapesMatches;
}

#endregion

#region STRUCTS

public struct GridShapeInfo
{
    public GameObject shapePrefab;
    public GameObject bonusPrefab;
}

#endregion

#region ENUMS

[Flags]
public enum BonusType
{
    None,
    DestroyWholeRowColumn
}

public enum GameRunningState
{
    None,
    SelectionStarted,
    ApplyingPowerUp,
    Animating,
    Clearing,
    Restrict_Controlls
}

public enum PowerUp
{
    None,
    RowColumn,
    ColorBomb,
    Hammer,
    Shuffle
}

#endregion