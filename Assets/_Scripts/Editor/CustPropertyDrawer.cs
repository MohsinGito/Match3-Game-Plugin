using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomPropertyDrawer(typeof(Grid2DLayout))]
public class CustPropertyDrawer : PropertyDrawer
{
    private int rows = 10;
    private int columns = 10;
    private int cellHieght = 25;
    private int cellWidth = 50;
    private int gridHieght = 20;
    private int gridWidth = 15;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        rows = property.FindPropertyRelative("rows").arraySize;
        columns = rows > 0 ? property.FindPropertyRelative("rows").GetArrayElementAtIndex(0).FindPropertyRelative("row").arraySize : 0;

        EditorGUI.PrefixLabel(position, label);
        Rect newposition = position;
        newposition.y += cellHieght;
        SerializedProperty data = property.FindPropertyRelative("rows");
        if (data.arraySize != rows)
            data.arraySize = rows;

        for (int j = 0; j < rows; j++)
        {
            SerializedProperty row = data.GetArrayElementAtIndex(j).FindPropertyRelative("row");
            newposition.height = cellHieght;
            if (row.arraySize != columns)
                row.arraySize = columns;

            newposition.width = cellWidth;
            for (int i = 0; i < columns; i++)
            {
                EditorGUI.PropertyField(newposition, row.GetArrayElementAtIndex(i), GUIContent.none);
                newposition.x += newposition.width;
            }

            newposition.x = position.x;
            newposition.y += cellHieght;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return gridHieght * gridWidth;
    }
}
#endif