using UnityEngine;
using System.Collections.Generic;

public class Match3Extensions : MonoBehaviour
{

    private GameLevel currentLevel;

    public void Init(GameLevel _level)
    {
        currentLevel = _level;
    }

    public bool AreVerticalOrHorizontalNeighbors(Shape s1, Shape s2)
    {
        return (s1.Column == s2.Column ||
                        s1.Row == s2.Row)
                        && Mathf.Abs(s1.Column - s2.Column) <= 1
                        && Mathf.Abs(s1.Row - s2.Row) <= 1;
    }

    public IEnumerable<GameObject> GetPotentialMatches(Shapes2DGrid shapes)
    {
        //list that will contain all the matches we find
        List<List<GameObject>> matches = new List<List<GameObject>>();

        for (int row = 0; row < currentLevel.Rows; row++)
        {
            for (int column = 0; column < currentLevel.Columns; column++)
            {
                var matches1 = CheckHorizontal1(row, column, shapes);
                var matches2 = CheckHorizontal2(row, column, shapes);
                var matches3 = CheckHorizontal3(row, column, shapes);
                var matches4 = CheckHorizontal4(row, column, shapes);
                var matches5 = CheckVertical1(row, column, shapes);
                var matches6 = CheckVertical2(row, column, shapes);
                var matches7 = CheckVertical3(row, column, shapes);
                var matches8 = CheckVertical4(row, column, shapes);

                if (matches1 != null) matches.Add(matches1);
                if (matches2 != null) matches.Add(matches2);
                if (matches3 != null) matches.Add(matches3);
                if (matches4 != null) matches.Add(matches4);
                if (matches5 != null) matches.Add(matches5);
                if (matches6 != null) matches.Add(matches6);
                if (matches7 != null) matches.Add(matches7);
                if (matches8 != null) matches.Add(matches8);

                //if we have >= 3 matches, return a random one
                if (matches.Count >= 3)
                {
                    return matches[Random.Range(0, matches.Count - 1)];
                }

                //if we are in the middle of the calculations/loops
                //and we have less than 3 matches, return a random one
                if (row >= currentLevel.Rows / 2 && matches.Count > 0 && matches.Count <= 2)
                {
                    return matches[Random.Range(0, matches.Count - 1)];
                }
            }
        }
        return null;
    }

    public List<GameObject> CheckHorizontal1(int row, int column, Shapes2DGrid shapes)
    {
        //  *
        //      *   *
        //
        //      *   *
        //  *
        if (column <= currentLevel.Columns - 2)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row, column + 1]))
            {
                if (row >= 1 && column >= 1)
                {
                    if (shapes.IsGridComponent(row, column - 1))
                    {
                        if (shapes.IsSame(shapes[row, column], shapes[row - 1, column - 1]) && shapes[row, column - 1].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        {
                            return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row - 1, column - 1]
                                };
                        }
                    }
                }

                if (row <= currentLevel.Rows - 2 && column >= 1)
                {
                    if (shapes.IsGridComponent(row, column - 1))
                    {
                        if (shapes.IsSame(shapes[row, column], shapes[row + 1, column - 1]) && shapes[row, column - 1].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        {
                            return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row + 1, column - 1]
                                };
                        }
                    } 
                }
            }
        }
        return null;
    }


    public List<GameObject> CheckHorizontal2(int row, int column, Shapes2DGrid shapes)
    {
        //         *
        // *   *
        //
        //  *   *
        //         *
        if (column <= currentLevel.Columns - 3)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row, column + 1]))
            {

                if (row >= 1 && column <= currentLevel.Columns - 3)
                    if (shapes.IsGridComponent(row, column + 2))
                        if (shapes.IsSame(shapes[row, column], shapes[row - 1, column + 2]) && shapes[row, column + 2].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row - 1, column + 2]
                                };

                if (row <= currentLevel.Rows - 2 && column <= currentLevel.Columns - 3)
                    if (shapes.IsGridComponent(row, column + 2))
                        if (shapes.IsSame(shapes[row, column], shapes[row + 1, column + 2]) && shapes[row, column + 2].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row + 1, column + 2]
                                };
            }
        }
        return null;
    }

    public List<GameObject> CheckHorizontal3(int row, int column, Shapes2DGrid shapes)
    {

        //   *        *   *
        //
        //   *   *        *
        if (column <= currentLevel.Columns - 4)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row, column + 1]) &&
                shapes.IsSame(shapes[row, column], shapes[row, column + 3]))
            {
                if (shapes.IsGridComponent(row, column + 2))
                    if(shapes[row, column + 2].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                    return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row, column + 3]
                                };
            }
        }
        if (column >= 2 && column <= currentLevel.Columns - 2)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row, column + 1]) &&
                shapes.IsSame(shapes[row, column], shapes[row, column - 2]))
            {
                if (shapes.IsGridComponent(row, column - 1))
                    if(shapes[row, column - 1].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                    return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row, column + 1],
                                    shapes[row, column -2]
                                };
            }
        }
        return null;
    }

    public List<GameObject> CheckHorizontal4(int row, int column, Shapes2DGrid shapes)
    {
        //      *  
        //  *       *
        //
        //  *       *
        //      *   

        if (column <= currentLevel.Columns - 3)
        {
            if (row >= 1)
            {
                if (shapes.IsSame(shapes[row, column], shapes[row - 1, column + 1]))
                {
                    if (shapes.IsSame(shapes[row, column], shapes[row, column + 2]))
                    {
                        if (shapes.IsGridComponent(row, column + 1))
                            if (shapes[row, column + 1].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row - 1, column + 1],
                                    shapes[row, column + 2]
                                };
                    }
                }
            }

            if (row <= currentLevel.Rows - 2)
            {
                if (shapes.IsSame(shapes[row, column], shapes[row + 1, column + 1]))
                {
                    if (shapes.IsSame(shapes[row, column], shapes[row, column + 2]))
                    {
                        if (shapes.IsGridComponent(row, column + 1))
                            if (shapes[row, column + 1].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column + 1],
                                    shapes[row, column + 2]
                                };
                    }
                }
            }
        }

        return null;
    }


    public List<GameObject> CheckVertical1(int row, int column, Shapes2DGrid shapes)
    {
        //      *       * 
        //      *       *
        //  *               *
        if (row <= currentLevel.Rows - 2)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row + 1, column]))
            {
                if (column >= 1 && row >= 1)
                    if (shapes.IsSame(shapes[row, column], shapes[row - 1, column - 1]))
                        if (shapes.IsGridComponent(row - 1, column))
                            if (shapes[row - 1, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column],
                                    shapes[row - 1, column -1]
                                };

                if (column <= currentLevel.Columns - 2 && row >= 1)
                    if (shapes.IsSame(shapes[row, column], shapes[row - 1, column + 1]))
                        if (shapes.IsGridComponent(row - 1, column))
                            if (shapes[row - 1, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column],
                                    shapes[row - 1, column + 1]
                                };
            }
        }
        return null;
    }

    public List<GameObject> CheckVertical2(int row, int column, Shapes2DGrid shapes)
    {
        //  *               *
        //      *       * 
        //      *       *
        if (row <= currentLevel.Rows - 3)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row + 1, column]))
            {
                if (column >= 1)
                    if (shapes.IsSame(shapes[row, column], shapes[row + 2, column - 1]))
                        if (shapes.IsGridComponent(row + 2, column))
                            if (shapes[row + 2, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column],
                                    shapes[row + 2, column -1]
                                };

                if (column <= currentLevel.Columns - 2)
                    if (shapes.IsSame(shapes[row, column], shapes[row + 2, column + 1]))
                        if (shapes.IsGridComponent(row + 2, column))
                            if (shapes[row + 2, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row+1, column],
                                    shapes[row + 2, column + 1]
                                };

            }
        }
        return null;
    }

    public List<GameObject> CheckVertical3(int row, int column, Shapes2DGrid shapes)
    {
        //      *       *
        //              *
        //      * 
        //      *       *
        if (row <= currentLevel.Rows - 4)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row + 1, column]) &&
                shapes.IsSame(shapes[row, column], shapes[row + 3, column]))
            {
                if (shapes.IsGridComponent(row + 2, column))
                    if (shapes[row + 2, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column],
                                    shapes[row + 3, column]
                                };
            }
        }

        if (row >= 2 && row <= currentLevel.Rows - 2)
        {
            if (shapes.IsSame(shapes[row, column], shapes[row + 1, column]) &&
                shapes.IsSame(shapes[row, column], shapes[row - 2, column]))
            {
                if (shapes.IsGridComponent(row - 1, column))
                    if (shapes[row - 1, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column],
                                    shapes[row - 2, column]
                                };
            }
        }
        return null;
    }

    public List<GameObject> CheckVertical4(int row, int column, Shapes2DGrid shapes)
    {
        //      *       *    
        //  *               *
        //      *       *
        if (row <= currentLevel.Rows - 3)
        {
            if (column >= 1)
            {
                if (shapes.IsSame(shapes[row, column], shapes[row + 1, column - 1]))
                {
                    if (shapes.IsSame(shapes[row, column], shapes[row + 2, column]))
                        if (shapes.IsGridComponent(row + 1, column))
                            if (shapes[row + 1, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column - 1],
                                    shapes[row + 2, column]
                                };

                }
            }

            else if (column <= currentLevel.Columns - 2)
            {
                if (shapes.IsSame(shapes[row, column], shapes[row + 1, column + 1]))
                {
                    if (shapes.IsSame(shapes[row, column], shapes[row + 2, column]))
                        if (shapes.IsGridComponent(row + 1, column))
                            if (shapes[row + 1, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                                return new List<GameObject>()
                                {
                                    shapes[row, column],
                                    shapes[row + 1, column + 1],
                                    shapes[row + 2, column]
                                };
                }
            }
        }
        return null;
    }

    public Vector2 ComputePosition(int row, int column)
    {
        float offsetX = (currentLevel.Columns - 1) * currentLevel.CellSize.x / 2;
        float offsetY = (currentLevel.Rows - 1) * currentLevel.CellSize.y / 2;

        return new Vector2(column * currentLevel.CellSize.x - offsetX,
                           row * currentLevel.CellSize.y - offsetY) + currentLevel.ScreenOffset;
    }

    public ShapeType GetShapeType(ShapeTypeEditor type)
    {
        switch (type)
        {
            case ShapeTypeEditor.RED:
                return ShapeType.RED;
            case ShapeTypeEditor.BLUE:
                return ShapeType.BLUE;
            case ShapeTypeEditor.GREEN:
                return ShapeType.GREEN;
            case ShapeTypeEditor.YELLOW:
                return ShapeType.YELLOW;
            case ShapeTypeEditor.PURPLE:
                return ShapeType.PURPLE;
        }

        return ShapeType.RED;
    }

    public bool CanSpawnBoardBomb()
    {
        if (currentLevel.BoardBombChances <= 0) return false;
        if (currentLevel.BoardBombChances >= 100) return true;

        int randomValue = Random.Range(1, 101);
        return randomValue <= currentLevel.BoardBombChances;
    }

}