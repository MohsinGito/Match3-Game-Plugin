using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class Shapes2DGrid : MonoBehaviour
{

    #region Initialization 

    private GameObject[,] shapes;
    private LevelsConfig levelsConfig;
    private GameLevel gameLevel;
    [HideInInspector] public bool[,] Grid2D;
    [HideInInspector] public GameObject[,] ShapesBgs;
    [HideInInspector] public ShapeTypeEditor[,] GridSpawns;
    [HideInInspector] public bool isPotentialMatchSearch;

    public ShapeMatchStats Stats { private set; get; }

    public void Init(GameLevel _level, LevelsConfig _levelsConfig)
    {
        gameLevel = _level;
        levelsConfig = _levelsConfig;
        shapes = new GameObject[gameLevel.Rows, gameLevel.Columns];
        Stats = new ShapeMatchStats();
    }

    public GameObject this[int row, int column]
    {
        get
        {
            try
            {
                return shapes[row, column];
            }
            catch (Exception ex)
            {

                Debug.LogError(row + " :: " + column);
                throw ex;
            }
        }
        set
        {
            shapes[row, column] = value;
        }
    }

    public bool IsSame(GameObject _shape1, GameObject _shape2)
    {
        if(_shape1 == null || _shape2 == null)
            return false;

        return _shape1.GetComponent<Shape>().IsSameType(_shape2.GetComponent<Shape>(), isPotentialMatchSearch);
    }

    public bool IsGridComponent(int row, int column)
    {
        return Grid2D[row, column];
    }

    public void ClearShapes()
    {
        foreach (GameObject _bg in ShapesBgs)
            Destroy(_bg);

        for (int i = 0; i < gameLevel.Rows; i++)
        {
            for (int j = 0; j < gameLevel.Columns; j++)
            {
                if (this[i, j] != null)
                    Destroy(this[i, j]);
            }
        }

    }

    #endregion

    #region Swapping Shapes

    private GameObject backupG1;
    private GameObject backupG2;

    public void Swap(GameObject g1, GameObject g2)
    {
        //hold a backup in case no match is produced
        backupG1 = g1;
        backupG2 = g2;

        var g1Shape = g1.GetComponent<Shape>();
        var g2Shape = g2.GetComponent<Shape>();

        //get array indexes
        int g1Row = g1Shape.Row;
        int g1Column = g1Shape.Column;
        int g2Row = g2Shape.Row;
        int g2Column = g2Shape.Column;

        //swap them in the array
        var temp = shapes[g1Row, g1Column];
        shapes[g1Row, g1Column] = shapes[g2Row, g2Column];
        shapes[g2Row, g2Column] = temp;

        //swap their respective properties
        Shape.SwapColumnRow(g1Shape, g2Shape);

    }

    public void UndoSwap()
    {
        if (backupG1 == null || backupG2 == null)
            throw new Exception("Backup is null");

        Swap(backupG1, backupG2);
    }

    #endregion

    #region Public Methods

    public IEnumerable<GameObject> GetMatches(IEnumerable<GameObject> gos)
    {
        List<GameObject> matches = new List<GameObject>();
        foreach (var go in gos)
        {
            if (go == null)
                continue;

            if (go.GetComponent<Shape>().SpecialPieceType != SpecialGridPiece.NONE)
                continue;

            matches.AddRange(GetMatches(go).MatchedShape);
        }
        return matches.Distinct();
    }

    public MatchesInfo GetMatches(GameObject go, bool containRowBonus = false)
    {
        MatchesInfo matchesInfo = new MatchesInfo();

        var horizontalMatches = GetMatchesHorizontally(go);
        if (ContainsDestroyRowColumnBonus(horizontalMatches) || containRowBonus)
        {
            horizontalMatches = GetEntireRow(go);
            Stats.horizontalBonusRow = GetBonusShape(horizontalMatches).GetComponent<Shape>().Row;

            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;
        }

        matchesInfo.AddObjectRange(horizontalMatches);
        var verticalMatches = GetMatchesVertically(go);
        if (ContainsDestroyRowColumnBonus(verticalMatches))
        {
            verticalMatches = GetEntireColumn(go);

            Stats.verticalBonusColumn = GetBonusShape(verticalMatches).GetComponent<Shape>().Column;

            if (!BonusTypeUtilities.ContainsDestroyWholeRowColumn(matchesInfo.BonusesContained))
                matchesInfo.BonusesContained |= BonusType.DestroyWholeRowColumn;
        }

        matchesInfo.AddObjectRange(verticalMatches);
        Stats.totalMatches = matchesInfo.MatchedShape.Count();
        return matchesInfo;
    }

    public IEnumerable<GameObject> GetSpecialTypesNearby(Shape shape, SpecialGridPiece targetType)
    {
        List<GameObject> matchingShapes = new List<GameObject>();

        if (shape.Row >= 0 && shape.Row < gameLevel.Rows && shape.Column >= 0 && shape.Column < gameLevel.Columns)
        {
            if (shape.Column > 0 && IsSameSpecialPiece(shapes[shape.Row, shape.Column - 1], targetType))
                matchingShapes.Add(shapes[shape.Row, shape.Column - 1]);

            if (shape.Column < gameLevel.Columns - 1 && IsSameSpecialPiece(shapes[shape.Row, shape.Column + 1], targetType))
                matchingShapes.Add(shapes[shape.Row, shape.Column + 1]);

            if (shape.Row > 0 && IsSameSpecialPiece(shapes[shape.Row - 1, shape.Column], targetType))
                matchingShapes.Add(shapes[shape.Row - 1, shape.Column]);

            if (shape.Row < gameLevel.Rows - 1 && IsSameSpecialPiece(shapes[shape.Row + 1, shape.Column], targetType))
                matchingShapes.Add(shapes[shape.Row + 1, shape.Column]);
        }
        else
        {
            Debug.LogError("Invalid array indices");
        }

        return matchingShapes;
    }

    public IEnumerable<GameObject> GetPiecesNearBoardBomb(Shape shape)
    {
        List<GameObject> matchingShapes = new List<GameObject>();

        if (shape.SpecialPieceType == SpecialGridPiece.SHELL || shape.SpecialPieceType == SpecialGridPiece.BLOCK)
            return matchingShapes;

        if (shape.Row >= 0 && shape.Row < gameLevel.Rows && shape.Column >= 0 && shape.Column < gameLevel.Columns)
        {
            if (shape.Column > 0 && shapes[shape.Row, shape.Column - 1] != null)
                matchingShapes.Add(shapes[shape.Row, shape.Column - 1]);

            if (shape.Column < gameLevel.Columns - 1 && shapes[shape.Row, shape.Column + 1] != null)
                matchingShapes.Add(shapes[shape.Row, shape.Column + 1]);

            if (shape.Row > 0 && shapes[shape.Row - 1, shape.Column] != null)
                matchingShapes.Add(shapes[shape.Row - 1, shape.Column]);

            if (shape.Row < gameLevel.Rows - 1 && shapes[shape.Row + 1, shape.Column] != null)
                matchingShapes.Add(shapes[shape.Row + 1, shape.Column]);

            if (shape.Row > 0 && shape.Column > 0 && shapes[shape.Row - 1, shape.Column - 1] != null)
                matchingShapes.Add(shapes[shape.Row - 1, shape.Column - 1]);

            if (shape.Row > 0 && shape.Column < gameLevel.Columns - 1 && shapes[shape.Row - 1, shape.Column + 1] != null)
                matchingShapes.Add(shapes[shape.Row - 1, shape.Column + 1]);

            if (shape.Row < gameLevel.Rows - 1 && shape.Column > 0 && shapes[shape.Row + 1, shape.Column - 1] != null)
                matchingShapes.Add(shapes[shape.Row + 1, shape.Column - 1]);

            if (shape.Row < gameLevel.Rows - 1 && shape.Column < gameLevel.Columns - 1 && shapes[shape.Row + 1, shape.Column + 1] != null)
                matchingShapes.Add(shapes[shape.Row + 1, shape.Column + 1]);
        }
        else
        {
            Debug.LogError("Invalid array indices");
        }

        return matchingShapes;
    }


    private bool IsSameSpecialPiece(GameObject _piece, SpecialGridPiece _type)
    {
        if (_piece == null)
            return false;

        if (_piece.GetComponent<Shape>().SpecialPieceType != _type)
            return false;

        return true;
    }

    public ModShapeInfo Collapse(IEnumerable<int> columns)
    {

        ModShapeInfo collapseInfo = new ModShapeInfo();

        ///search in every column
        foreach (var column in columns)
        {
            //begin from bottom row
            for (int row = 0; row < gameLevel.Rows - 1; row++)
            {
                //if you find a null item
                if (shapes[row, column] == null && IsGridComponent(row, column))
                {
                    //start searching for the first non-null
                    for (int row2 = row + 1; row2 < gameLevel.Rows; row2++)
                    {
                        //if you find one, bring it down (i.e. replace it with the null you found)
                        if (shapes[row2, column] != null)
                        {
                            if (shapes[row2, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.BLOCK || shapes[row2, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.SHELL)
                                continue;

                            shapes[row, column] = shapes[row2, column];
                            shapes[row2, column] = null;

                            //calculate the biggest distance
                            if (row2 - row > collapseInfo.MaxDistance)
                                collapseInfo.MaxDistance = row2 - row;

                            //assign new row and column (name does not change)
                            shapes[row, column].GetComponent<Shape>().Row = row;
                            shapes[row, column].GetComponent<Shape>().Column = column;

                            collapseInfo.AddShape(shapes[row, column]);
                            break;
                        }
                    }
                }
            }
        }

        return collapseInfo;
    }

    public IEnumerable<ShapeInfo> GetEmptyItemsOnColumn(int column)
    {
        List<ShapeInfo> emptyItems = new List<ShapeInfo>();
        for (int row = 0; row < gameLevel.Rows; row++)
        {
            if (shapes[row, column] == null && IsGridComponent(row, column))
                emptyItems.Add(new ShapeInfo() { Row = row, Column = column });
        }
        return emptyItems;
    }

    public List<Shape> GetRow(int row)
    {
        List<Shape> shapesRow = new List<Shape>();
        for (int i = 0; i < gameLevel.Columns; i++)
        {
            if (shapes[row, i] != null)
                shapesRow.Add(shapes[row, i].GetComponent<Shape>());
        }
        return shapesRow;
    }

    public List<Shape> GetColumn(int column)
    {
        List<Shape> shapesColumn = new List<Shape>();
        for (int i = 0; i < gameLevel.Rows; i++)
        {
            if (shapes[i, column] != null)
                shapesColumn.Add(shapes[i, column].GetComponent<Shape>());
        }
        return shapesColumn;
    }

    public void Remove(GameObject item)
    {
        shapes[item.GetComponent<Shape>().Row, item.GetComponent<Shape>().Column] = null;
    }

    public List<GameObject> GetAllSameColorShapes(ShapeType _shape)
    {
        List<GameObject> colorsShapes = new List<GameObject>();
        for (int row = 0; row < gameLevel.Rows; row++)
        {
            for (int column = 0; column < gameLevel.Columns; column++)
            {
                if (shapes[row, column] == null) 
                    continue;

                if (shapes[row, column].GetComponent<Shape>().SpecialPieceType != SpecialGridPiece.NONE)
                    continue;

                if (shapes[row, column].GetComponent<Shape>().ShapeType == _shape) 
                    colorsShapes.Add(shapes[row, column]);
            }
        }
        
        return colorsShapes;
    }

    #endregion

    #region Private Methods

    private bool ContainsDestroyRowColumnBonus(IEnumerable<GameObject> matches)
    {
        if (matches.Count() >= levelsConfig.MinimumMatches)
        {
            foreach (var go in matches)
            {
                if (go == null)
                    continue;

                if (BonusTypeUtilities.ContainsDestroyWholeRowColumn
                    (go.GetComponent<Shape>().Bonus))
                    return true;
            }
        }

        return false;
    }

    private GameObject GetBonusShape(IEnumerable<GameObject> matches)
    {
        //if (matches.Count() >= levelsConfig.MinimumMatches)
        {
            foreach (var go in matches)
            {
                if (go == null) 
                    continue;

                if (BonusTypeUtilities.ContainsDestroyWholeRowColumn
                    (go.GetComponent<Shape>().Bonus))
                    return go;
            }
        }

        return null;
    }

    private IEnumerable<GameObject> GetEntireRow(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int row = go.GetComponent<Shape>().Row;
        for (int column = 0; column < gameLevel.Columns; column++)
        {
            if (shapes[row, column] == null)
                continue;

            if (shapes[row, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE) 
                matches.Add(shapes[row, column]);
        }
        return matches;
    }

    private IEnumerable<GameObject> GetEntireColumn(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        int column = go.GetComponent<Shape>().Column;
        for (int row = 0; row < gameLevel.Rows; row++)
        {
            if (shapes[row, column] == null)
                continue;

            if (shapes[row, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                    matches.Add(shapes[row, column]);
        }
        return matches;
    }

    private IEnumerable<GameObject> GetMatchesHorizontally(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<Shape>();
        //check left
        if (shape.Column != 0)
            for (int column = shape.Column - 1; column >= 0; column--)
            {
                if (IsSame(shapes[shape.Row, column], go))
                {
                    matches.Add(shapes[shape.Row, column]);
                }
                else
                    break;
            }

        //check right
        if (shape.Column != gameLevel.Columns - 1)
            for (int column = shape.Column + 1; column < gameLevel.Columns; column++)
            {
                if (IsSame(shapes[shape.Row, column], go))
                {
                    matches.Add(shapes[shape.Row, column]);
                }
                else
                    break;
            }

        //we want more than three matches
        if (matches.Count < levelsConfig.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }

    private IEnumerable<GameObject> GetMatchesVertically(GameObject go)
    {
        List<GameObject> matches = new List<GameObject>();
        matches.Add(go);
        var shape = go.GetComponent<Shape>();
        //check bottom
        if (shape.Row != 0)
            for (int row = shape.Row - 1; row >= 0; row--)
            {
                if (shapes[row, shape.Column] != null &&
                    IsSame(shapes[row, shape.Column], go))
                {
                    matches.Add(shapes[row, shape.Column]);
                }
                else
                    break;
            }

        //check top
        if (shape.Row != gameLevel.Rows - 1)
            for (int row = shape.Row + 1; row < gameLevel.Rows; row++)
            {
                if (shapes[row, shape.Column] != null && IsSame(shapes[row, shape.Column], go))
                {
                    matches.Add(shapes[row, shape.Column]);
                }
                else
                    break;
            }


        if (matches.Count < levelsConfig.MinimumMatches)
            matches.Clear();

        return matches.Distinct();
    }


    #endregion
    
}

public class ShapeMatchStats
{
    public int totalMatches;
    public int horizontalBonusRow;
    public int verticalBonusColumn;

    public ShapeMatchStats()
    {
        Reset();
    }

    public void Reset()
    {
        totalMatches = 0;
        horizontalBonusRow = verticalBonusColumn = -1;
    }
}