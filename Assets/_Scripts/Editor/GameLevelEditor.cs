using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(GameLevel))]
public class GameLevelEditor : Editor
{
    SerializedProperty rowsProp;
    SerializedProperty columnsProp;
    SerializedProperty levelGridProp;
    SerializedProperty cellSizesProp;
    SerializedProperty screenOffsetProp;
    SerializedProperty shapeSizeFactorProp;
    SerializedProperty randomizeShapesProp;
    SerializedProperty gridShapeProp;
    SerializedProperty scoreTargetProp;
    SerializedProperty levelShapesProp;
    SerializedProperty levelCompletedProp;
    SerializedProperty scoresEarnedProp;
    SerializedProperty BoardBombChancesProp;
    SerializedProperty canAddBonusProp;
    SerializedProperty canShowHintProp;
    SerializedProperty levelMovesProp;
    SerializedProperty movesCountProp;
    SerializedProperty containsJellyBlocksProp;
    SerializedProperty jellyRowAndColumnProp;
    SerializedProperty containsRockBlocksProp;
    SerializedProperty rockRowAndColumnsProp;
    SerializedProperty timeLevelProp;
    SerializedProperty levelDurationProp;
    SerializedProperty targetScoresProp;
    SerializedProperty matchShapeProp;
    SerializedProperty shapesMatchesProp;
    bool[,] previousNormalGridValues;
    bool[,] previousJellyGridValues;
    bool[,] previousRockGridValues;

    int gridRow, gridColumn;
    GameLevel gameLevel;

    private void OnEnable()
    {
        gameLevel = (GameLevel)target;
        rowsProp = serializedObject.FindProperty("Rows");
        columnsProp = serializedObject.FindProperty("Columns");
        levelGridProp = serializedObject.FindProperty("LevelGrid");
        cellSizesProp = serializedObject.FindProperty("CellSize");
        screenOffsetProp = serializedObject.FindProperty("ScreenOffset");
        shapeSizeFactorProp = serializedObject.FindProperty("ShapeSizeFactor");
        randomizeShapesProp = serializedObject.FindProperty("RandomizeColorShapes");
        gridShapeProp = serializedObject.FindProperty("GridShapes");
        scoreTargetProp = serializedObject.FindProperty("ScoreTarget");
        levelShapesProp = serializedObject.FindProperty("LevelShapes");
        levelCompletedProp = serializedObject.FindProperty("LevelCompleted");
        scoresEarnedProp = serializedObject.FindProperty("ScoresEarned");
        BoardBombChancesProp = serializedObject.FindProperty("BoardBombChances");
        canAddBonusProp = serializedObject.FindProperty("CanAddBonus");
        canShowHintProp = serializedObject.FindProperty("CanShowHint");
        levelMovesProp = serializedObject.FindProperty("LevelMoves");
        movesCountProp = serializedObject.FindProperty("MovesCount");
        containsJellyBlocksProp = serializedObject.FindProperty("ContainsJellyBlocks");
        jellyRowAndColumnProp = serializedObject.FindProperty("JellyRowAndColumn");
        containsRockBlocksProp = serializedObject.FindProperty("ContainsRockBlocks");
        rockRowAndColumnsProp = serializedObject.FindProperty("RockRowAndColumns");
        timeLevelProp = serializedObject.FindProperty("TimeLevel");
        levelDurationProp = serializedObject.FindProperty("LevelDuration");
        targetScoresProp = serializedObject.FindProperty("TargetScores");
        matchShapeProp = serializedObject.FindProperty("ShapesMatchTarget");
        shapesMatchesProp = serializedObject.FindProperty("ShapesMatches");
        levelGridProp = serializedObject.FindProperty("LevelGrid");
        SetupGrids();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Grid Size Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(rowsProp, new GUIContent("Rows"));
        EditorGUILayout.PropertyField(columnsProp, new GUIContent("Columns"));

        rowsProp.intValue = Mathf.Clamp(rowsProp.intValue, 4, 10);
        columnsProp.intValue = Mathf.Clamp(columnsProp.intValue, 4, 10);

        if (GUILayout.Button("Create Grid"))
        {
            SetupGrids(); 
            gameLevel.PopulateGridShapes();
            levelShapesProp.arraySize = System.Enum.GetValues(typeof(ShapeType)).Length;
            for (int i = 0; i < levelShapesProp.arraySize; i++)
            {
                levelShapesProp.GetArrayElementAtIndex(i).enumValueIndex = i;
            }
        }

        if (levelGridProp.FindPropertyRelative("rows").arraySize > 0)
        {
            SynchronizeGrids(); 
            EditorGUILayout.Space();
            //EditorGUILayout.PropertyField(levelGridProp);

            EditorGUILayout.PropertyField(randomizeShapesProp);
            //if (!randomizeShapesProp.boolValue)
            {
                DrawGridShapesWithoutAddRemove(serializedObject.FindProperty("GridShapes.gridRows"));
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(cellSizesProp);
            EditorGUILayout.PropertyField(screenOffsetProp, new GUIContent("Grid Offset"));
            EditorGUILayout.PropertyField(shapeSizeFactorProp, new GUIContent("Shape Size Factor"));
            EditorGUILayout.PropertyField(levelShapesProp, true);
            EditorGUILayout.PropertyField(BoardBombChancesProp, new GUIContent("Board Bomb Chances"));
            EditorGUILayout.PropertyField(canAddBonusProp);
            EditorGUILayout.PropertyField(canShowHintProp);

            EditorGUILayout.PropertyField(levelMovesProp, new GUIContent("Moves"));
            if (levelMovesProp.enumValueIndex == (int)Moves.Limited)
            {
                EditorGUILayout.PropertyField(movesCountProp, new GUIContent("Moves Count"));
                EditorGUILayout.Space();
            }
            EditorGUILayout.PropertyField(timeLevelProp, new GUIContent("Duration"));
            if (timeLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(levelDurationProp, new GUIContent("Level Duration"));
            }

            EditorGUILayout.PropertyField(matchShapeProp, new GUIContent("Matches Target"));
            if (matchShapeProp.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(shapesMatchesProp, true);
                EditorGUILayout.Space();
            }
            EditorGUILayout.PropertyField(targetScoresProp, new GUIContent("Target Scores"));
            if (targetScoresProp.boolValue)
            {
                EditorGUILayout.PropertyField(scoreTargetProp);
                EditorGUILayout.Space();
            }

            EditorGUILayout.PropertyField(levelCompletedProp);
            EditorGUILayout.PropertyField(scoresEarnedProp);

        }
        serializedObject.ApplyModifiedProperties();
    }

    private void SetupGrids()
    {
        if (rowsProp.intValue <= 0 || columnsProp.intValue <= 0)
        {
            Debug.LogError("Rows and Columns must be between 1 and 10");
            return;
        }

        gridRow = rowsProp.intValue;
        gridColumn = columnsProp.intValue;

        previousNormalGridValues = new bool[rowsProp.intValue, columnsProp.intValue];
        previousJellyGridValues = new bool[rowsProp.intValue, columnsProp.intValue];
        previousRockGridValues = new bool[rowsProp.intValue, columnsProp.intValue];

        levelGridProp.FindPropertyRelative("rows").arraySize = rowsProp.intValue;
        jellyRowAndColumnProp.FindPropertyRelative("rows").arraySize = rowsProp.intValue;
        rockRowAndColumnsProp.FindPropertyRelative("rows").arraySize = rowsProp.intValue;
        gridShapeProp.FindPropertyRelative("gridRows").arraySize = rowsProp.intValue;
        for (int i = 0; i < rowsProp.intValue; i++)
        {
            SerializedProperty gridRow = levelGridProp.FindPropertyRelative("rows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
            SerializedProperty jellyRow = jellyRowAndColumnProp.FindPropertyRelative("rows").GetArrayElementAtIndex(i).FindPropertyRelative("row");
            SerializedProperty rockRow = rockRowAndColumnsProp.FindPropertyRelative("rows").GetArrayElementAtIndex(i).FindPropertyRelative("row"); SerializedProperty gridShapeRow = gridShapeProp.FindPropertyRelative("gridRows").GetArrayElementAtIndex(i).FindPropertyRelative("shapesRow");


            gridRow.arraySize = columnsProp.intValue;
            jellyRow.arraySize = columnsProp.intValue;
            rockRow.arraySize = columnsProp.intValue;
            gridShapeRow.arraySize = columnsProp.intValue;

            for (int j = 0; j < columnsProp.intValue; j++)
            {
                previousNormalGridValues[i, j] = gridRow.GetArrayElementAtIndex(j).boolValue = true;
                previousJellyGridValues[i, j] = jellyRow.GetArrayElementAtIndex(j).boolValue = false;
                previousRockGridValues[i, j] = rockRow.GetArrayElementAtIndex(j).boolValue = false;
            }
        }
    }

    private void SynchronizeGrids()
    {
        SerializedProperty normalGridArr = levelGridProp.FindPropertyRelative("rows");
        SerializedProperty jellyGridArr = jellyRowAndColumnProp.FindPropertyRelative("rows");
        SerializedProperty rockGridArr = rockRowAndColumnsProp.FindPropertyRelative("rows");

        for (int i = 0; i < gridRow; i++)
        {
            SerializedProperty normalRow = null, jellyRow = null, rockRow = null;
            normalRow = normalGridArr.GetArrayElementAtIndex(i).FindPropertyRelative("row");
            if (containsJellyBlocksProp.boolValue)
                jellyRow = jellyGridArr.GetArrayElementAtIndex(i).FindPropertyRelative("row");
            if (containsRockBlocksProp.boolValue)
                rockRow = rockGridArr.GetArrayElementAtIndex(i).FindPropertyRelative("row");

            for (int j = 0; j < gridColumn; j++)
            {
                if (previousNormalGridValues != null)
                {
                    if (normalRow.GetArrayElementAtIndex(j).boolValue != previousNormalGridValues[i, j])
                    {
                        previousNormalGridValues[i, j] = normalRow.GetArrayElementAtIndex(j).boolValue;


                        if (previousNormalGridValues[i, j] && containsJellyBlocksProp.boolValue)
                        {
                            previousJellyGridValues[i, j] = false;
                            jellyRow.GetArrayElementAtIndex(j).boolValue = false;
                        }
                        if (previousNormalGridValues[i, j] && containsRockBlocksProp.boolValue)
                        {
                            previousRockGridValues[i, j] = false;
                            rockRow.GetArrayElementAtIndex(j).boolValue = false;
                        }
                    }
                }

                if (previousJellyGridValues != null && containsJellyBlocksProp.boolValue)
                {
                    if (jellyRow.GetArrayElementAtIndex(j).boolValue != previousJellyGridValues[i, j])
                    {
                        previousJellyGridValues[i, j] = jellyRow.GetArrayElementAtIndex(j).boolValue;

                        if (previousJellyGridValues[i, j])
                        {
                            previousNormalGridValues[i, j] = false;
                            normalRow.GetArrayElementAtIndex(j).boolValue = false;
                        }

                        if (previousJellyGridValues[i, j] && containsRockBlocksProp.boolValue)
                        {
                            previousRockGridValues[i, j] = false;
                            rockRow.GetArrayElementAtIndex(j).boolValue = false;
                        }
                    }
                }

                if (previousRockGridValues != null && containsRockBlocksProp.boolValue)
                {
                    if (rockRow.GetArrayElementAtIndex(j).boolValue != previousRockGridValues[i, j])
                    {
                        previousRockGridValues[i, j] = rockRow.GetArrayElementAtIndex(j).boolValue;

                        if (previousRockGridValues[i, j])
                        {
                            previousNormalGridValues[i, j] = false;
                            normalRow.GetArrayElementAtIndex(j).boolValue = false;
                        }

                        if (previousRockGridValues[i, j] && containsJellyBlocksProp.boolValue)
                        {
                            previousJellyGridValues[i, j] = false;
                            jellyRow.GetArrayElementAtIndex(j).boolValue = false;
                        }
                    }
                }
            }
        }
    }

    private void DrawGridShapesWithoutAddRemove(SerializedProperty gridShapes)
    {
        GUILayout.Label("Grid Initial Spawns");

        for (int i = 0; i < gridShapes.arraySize; i++)
        {
            SerializedProperty row = gridShapes.GetArrayElementAtIndex(i);
            SerializedProperty shapesRow = row.FindPropertyRelative("shapesRow");

            // Get the corresponding row from levelGrid
            SerializedProperty levelRow = levelGridProp.FindPropertyRelative("rows").GetArrayElementAtIndex(i);
            SerializedProperty levelRowData = levelRow.FindPropertyRelative("row");

            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < shapesRow.arraySize; j++)
            {
                SerializedProperty shape = shapesRow.GetArrayElementAtIndex(j);

                // Check if the corresponding position in levelGrid is true
                bool isEditable = levelRowData.GetArrayElementAtIndex(j).boolValue;

                // Save current GUI enabled state
                bool prevGUIState = GUI.enabled;

                // Set GUI enabled state based on isEditable
                GUI.enabled = isEditable;

                // Draw the property
                EditorGUILayout.PropertyField(shape, GUIContent.none);

                // Restore the previous GUI enabled state
                GUI.enabled = prevGUIState;
            }
            EditorGUILayout.EndHorizontal();
        }
    }

}

#endif