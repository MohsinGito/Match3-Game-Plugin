using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class Match3Events : MonoBehaviour
{

    [field: SerializeField] public LevelMajorEvents LevelEvents { private set; get; }
    [field: SerializeField] public LevelGameplayEvents LevelGameplayEvents { private set; get; }
    [field: SerializeField] public OjectiveEvents ObjectivesEvents { private set; get; }

}

[Serializable]
public class IntEvent : UnityEvent<int> { }

[Serializable]
public class LevelEvent : UnityEvent<GameLevel> { }

[Serializable]
public class MacthEvent : UnityEvent<string, int> { }

[Serializable]
public class ShapesEvent : UnityEvent<List<string>> { }

[Serializable]
public class ShapeVector3Event : UnityEvent<string, Vector3> { }

[Serializable]
public class ShapeRowColumnEvent : UnityEvent<int, Vector3, Vector3> { }

[Serializable]
public class MultipleShapeEvent : UnityEvent<string, Vector3, List<Vector3>> { };

[Serializable]
public class OjectiveEvents
{
    public IntEvent OnScoresChanged;
    public IntEvent OnNumberOfMovesLeftChanged;
    public IntEvent OnRemainingShellsCountChanged;
    public IntEvent OnRemainingBlocksCountChanged;
    public MacthEvent OnShapeColorMatchEvent;
    public UnityEvent OnAllObjectivesCleared;
}

[Serializable]
public class LevelMajorEvents
{
    public LevelEvent OnLevelStart;
    public UnityEvent OnLevelEnd;
    public UnityEvent OnSuccessfullMove;
    public UnityEvent OnUnSuccessfullMove;
    public ShapesEvent OnColorMatchOccurred;
    public MacthEvent OnShapesMatched;
    public UnityEvent OnShapesShuffled;
    public IntEvent OnRocksDestroyed;
    public IntEvent OnShellsDestroyed;
    public UnityEvent OnBombActivated;
}

[Serializable]
public class LevelGameplayEvents
{
    public ShapeVector3Event OnShapeDestroyed;
    public ShapeRowColumnEvent OnRowDestroyed;
    public ShapeRowColumnEvent OnColumnDestroyed;
    public MultipleShapeEvent OnBombActivation;
}