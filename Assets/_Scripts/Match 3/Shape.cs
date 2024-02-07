using System;
using UnityEngine;

public class Shape : MonoBehaviour
{

    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer shell;

    public int Row { get; set; }
    public int Column { get; set; }
    public BonusType Bonus { get; set; }
    [field: SerializeField] public ShapeType ShapeType { get; set; }
    public SpecialGridPiece SpecialPieceType { get; set; }

    public string Type { get; set; }

    public Shape()   
    {
        Bonus = BonusType.None;
    }

    public bool IsSameType(Shape otherShape, bool isPotentialMatch)
    {
        if (otherShape == null || !(otherShape is Shape))
            throw new ArgumentException("otherShape");

        if ((otherShape.SpecialPieceType == SpecialGridPiece.SHELL || SpecialPieceType == SpecialGridPiece.SHELL) && !isPotentialMatch)
            return string.Compare(this.Type, (otherShape as Shape).Type) == 0;

        if (otherShape.SpecialPieceType != SpecialGridPiece.NONE || SpecialPieceType != SpecialGridPiece.NONE)
            return false;

        return string.Compare(this.Type, (otherShape as Shape).Type) == 0;
    }

    public void SetSprite(Sprite sprite, int sortingOrder = 1)
    {
        body.sprite = sprite;
        body.sortingOrder = sortingOrder;
    }

    public void Assign(string type, int row, int column)
    {
        if (string.IsNullOrEmpty(type))
            throw new ArgumentException("type");

        Column = column;
        Row = row;
        Type = type;
        SpecialPieceType = SpecialGridPiece.NONE;
    }

    public void AssignSpecial(SpecialGridPiece type, int row, int column, string shapeType = "")
    {
        Column = column;
        Row = row;
        SpecialPieceType = type;
        if (!string.IsNullOrEmpty(shapeType)) Type = shapeType;
    }

    public void SetDisplayShell(Sprite shellSprite)
    {
        shell.sprite = shellSprite;
    }

    public void RemoveShell()
    {
        SpecialPieceType = SpecialGridPiece.NONE;
        shell.sprite = null;
    }

    public static void SwapColumnRow(Shape a, Shape b)
    {
        int temp = a.Row;
        a.Row = b.Row;
        b.Row = temp;

        temp = a.Column;
        a.Column = b.Column;
        b.Column = temp;
    }

}

public class BombEffectedShape
{
    public Shape shape;
    public ExplosionDirection direction;
}

public enum ExplosionDirection
{
    Horizontal,
    Vertical,
    DaigonalLeft,
    DaigonalRight
}