using DG.Tweening;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Match3Events))]
public class Match3Manager : MonoBehaviour
{

    #region Public Attributes

    [field: SerializeField] public LevelsConfig LevelsConfig { private set; get; }

    #endregion

    #region Private Attributes

    private bool gridInitialized;
    private bool isBombApplied;
    private GameLevel gameLevel;
    private Transform gridParent;
    private Transform piecesParent;
    private GameObject hitGo = null;
    private Match3Events events;
    private Vector2[] SpawnPositions;
    private IEnumerator CheckPotentialMatchesCoroutine;
    private IEnumerable<GameObject> potentialMatches;
    private Dictionary<ShapeType, GridShapeInfo> shapesDict;
    private GameRunningState state = GameRunningState.None;
    private PowerUp currentPowerUp = PowerUp.None;

    #endregion

    #region Initialization Methods

    public GameRunningState State { get { return state; } }
    public PowerUp CurrentPowerUp { get { return currentPowerUp; } }
    public List<Vector2> ShellsRowAndColumn { private set; get; }
    public List<Vector2> BlockRowAndColumn { private set; get; }
    public Shapes2DGrid Shapes { private set; get; }
    public Match3Extensions Match3Extensions { private set; get; }
    public Match3ObjectivesTracker ObjectivesTracker { private set; get; }

    private void Awake()
    {
        gridParent = new GameObject().transform;
        piecesParent = new GameObject().transform;
        gridParent.name = "Gameplay Grid";
        piecesParent.name = "Gems Pieces";

        Shapes = gameObject.AddComponent<Shapes2DGrid>();
        Match3Extensions = gameObject.AddComponent<Match3Extensions>();
        ObjectivesTracker = gameObject.AddComponent<Match3ObjectivesTracker>();
        events = GetComponent<Match3Events>();

        if (events == null)
            Debug.LogError("Require Component Match3Extensions Is Missing");
    }

    public void StartLevel(int _levelIndex)
    {
        if (gameLevel != null)
        {
            if (gameLevel == LevelsConfig.GetLevel(_levelIndex))
                ClearGrid();
        }

        currentPowerUp = PowerUp.None;
        state = GameRunningState.None;
        gameLevel = LevelsConfig.GetLevel(_levelIndex);
        ObjectivesTracker.Init(this, LevelsConfig, events);
        Shapes.Init(gameLevel, LevelsConfig);

        InitializeTypesOnPrefabShapesAndBonuses();
        InitializeShapesAndSpawnPositions();
        StartCheckForPotentialMatches();
        AnimateGridOnPlayerScreen();

        events.LevelEvents.OnLevelStart?.Invoke(gameLevel);
    }

    public void EndLevel()
    {
        gridInitialized = false;
        StopCheckForPotentialMatches();
        events.LevelEvents.OnLevelEnd?.Invoke(); 
    }

    public void PauseLevel() => gridInitialized = false;

    public void ResumeLevel() => gridInitialized = true;

    public void ClearGrid()
    {
        foreach (KeyValuePair<ShapeType, GridShapeInfo> _info in shapesDict)
        {
            Destroy(_info.Value.shapePrefab);
            Destroy(_info.Value.bonusPrefab);
        }

        Shapes.ClearShapes();
        ObjectivesTracker.ClearEvents();
        piecesParent.position = Vector3.zero;

        DOTween.KillAll();
        StopAllCoroutines();
    }

    public void ApplyPowerUp(PowerUp powerUp)
    {
        currentPowerUp = powerUp;
    }

    private void InitializeTypesOnPrefabShapesAndBonuses()
    {
        shapesDict = new Dictionary<ShapeType, GridShapeInfo>();
        foreach (ShapeType shapeType in gameLevel.LevelShapes)
        {
            GridShapeInfo newGridShape = new GridShapeInfo();
            newGridShape.shapePrefab = Instantiate(LevelsConfig.ShapePrefab, new Vector3(0, -8f, 0), Quaternion.identity);
            newGridShape.bonusPrefab = Instantiate(LevelsConfig.ShapePrefab, new Vector3(0, -8f, 0), Quaternion.identity);

            newGridShape.shapePrefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = LevelsConfig.GetShapeInfo(shapeType).shapeSprite;
            newGridShape.bonusPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = LevelsConfig.GetShapeInfo(shapeType).bonusSprite;

            newGridShape.shapePrefab.GetComponent<Shape>().ShapeType = shapeType;
            newGridShape.shapePrefab.GetComponent<Shape>().Type = shapeType.ToString();
            newGridShape.shapePrefab.GetComponent<Shape>().SpecialPieceType = SpecialGridPiece.NONE;
            newGridShape.bonusPrefab.GetComponent<Shape>().ShapeType = shapeType;
            newGridShape.bonusPrefab.GetComponent<Shape>().Type = shapeType.ToString(); 
            newGridShape.bonusPrefab.GetComponent<Shape>().SpecialPieceType = SpecialGridPiece.NONE;

            newGridShape.shapePrefab.transform.parent = transform;
            newGridShape.bonusPrefab.transform.parent = transform;
            shapesDict[shapeType] = newGridShape;
        }

        Match3Extensions.Init(gameLevel);
    }

    public void InitializeShapesAndSpawnPositions()
    {
        SpawnPositions = new Vector2[gameLevel.Columns];
        Shapes.Grid2D = new bool[gameLevel.Rows, gameLevel.Columns];
        Shapes.GridSpawns = new ShapeTypeEditor[gameLevel.Rows, gameLevel.Columns];
        Shapes.ShapesBgs = new GameObject[gameLevel.Rows, gameLevel.Columns];

        BlockRowAndColumn = new List<Vector2>();
        ShellsRowAndColumn = new List<Vector2>();

        int k = 0;
        for (int i = gameLevel.Rows - 1; i >= 0 ; i--)
        {
            for (int j = 0; j < gameLevel.Columns; j++)
            {
                Shapes.Grid2D[k, j] = gameLevel.GridShapes.gridRows[i].shapesRow[j] != ShapeTypeEditor.EMPTY;
                Shapes.GridSpawns[k, j] = gameLevel.GridShapes.gridRows[i].shapesRow[j];

                if (gameLevel.GridShapes.gridRows[i].shapesRow[j] == ShapeTypeEditor.SHELL)
                {
                    Shapes.Grid2D[k, j] = true;
                    ShellsRowAndColumn.Add(new Vector2(k, j));
                }

                if (gameLevel.GridShapes.gridRows[i].shapesRow[j] == ShapeTypeEditor.BLOCK)
                {
                    Shapes.Grid2D[k, j] = true;
                    BlockRowAndColumn.Add(new Vector2(k, j));
                }
            }
            k += 1;
        }

        if (Shapes != null)
            DestroyAllShapes();

        for (int row = 0; row < gameLevel.Rows; row++)
        {
            for (int column = 0; column < gameLevel.Columns; column++)
            { 
                if (Shapes.GridSpawns[row, column] == ShapeTypeEditor.EMPTY)
                {
                    Shapes[row, column] = null;
                    continue;
                }

                else if (Shapes.GridSpawns[row, column] == ShapeTypeEditor.BLOCK)
                {
                    Shapes[row, column] = InstantiateAndPlaceNewSpecialPiece(row, column, GetRandomShape(), SpecialGridPiece.BLOCK);
                }

                else if (Shapes.GridSpawns[row, column] == ShapeTypeEditor.SHELL)
                {
                    Shapes[row, column] = InstantiateAndPlaceNewSpecialPiece(row, column, GetRandomShapeForRolColumn(row, column), SpecialGridPiece.SHELL);
                }

                else if (Shapes.GridSpawns[row, column] == ShapeTypeEditor.BOARD_BOMB)
                {
                    Shapes[row, column] = InstantiateAndPlaceNewSpecialPiece(row, column, GetRandomShape(), SpecialGridPiece.BOARD_BOMB);
                }

                else
                {
                    GameObject newShape = gameLevel.RandomizeColorShapes ?
                    GetRandomShapeForRolColumn(row, column) : GetShape(Shapes.GridSpawns[row, column]);
                    InstantiateAndPlaceNewShape(row, column, newShape);
                }

                CreateShapeBackground(row, column);
            }
        }

        SetupSpawnPositions();
        gridInitialized = true;
    }

    private void CreateShapeBackground(int _row, int _column)
    {
        Shapes.ShapesBgs[_row, _column] = Instantiate(LevelsConfig.ShapeBg, Shapes[_row, _column].transform.position
                        , Quaternion.identity);
        Shapes.ShapesBgs[_row, _column].transform.parent = gridParent;
        Shapes.ShapesBgs[_row, _column].transform.localScale = gameLevel.CellSize;
    }

    private void InstantiateAndPlaceNewShape(int row, int column, GameObject newShape)
    {
        Vector2 position = Match3Extensions.ComputePosition(row, column);

        GameObject go = Instantiate(newShape, position, Quaternion.identity) as GameObject;
        go.transform.parent = piecesParent;
        go.transform.localScale = gameLevel.CellSize * gameLevel.ShapeSizeFactor;

        go.GetComponent<Shape>().Assign(newShape.GetComponent<Shape>().Type, row, column);
        Shapes[row, column] = go;
    }

    private GameObject InstantiateAndPlaceNewSpecialPiece(int row, int column, GameObject piece, SpecialGridPiece specialType)
    {
        Vector2 position = Match3Extensions.ComputePosition(row, column);
        Shape go = Instantiate(piece, position, Quaternion.identity).GetComponent<Shape>();

        switch (specialType)
        {
            case SpecialGridPiece.BLOCK:
                go.AssignSpecial(SpecialGridPiece.BLOCK, row, column);
                go.SetSprite(LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.BLOCK).DisplaySprite, 0);
                break;

            case SpecialGridPiece.SHELL:
                go.AssignSpecial(SpecialGridPiece.SHELL, row, column, piece.GetComponent<Shape>().Type);
                go.SetDisplayShell(LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.SHELL).DisplaySprite);
                break;

            case SpecialGridPiece.BOARD_BOMB:
                go.AssignSpecial(SpecialGridPiece.BOARD_BOMB, row, column);
                go.SetSprite(LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.BOARD_BOMB).DisplaySprite, 0);
                break;

                // Add other cases if you have more SpecialGridPiece types
        }

        go.transform.parent = piecesParent;
        go.transform.localScale = gameLevel.CellSize * gameLevel.ShapeSizeFactor;
        return go.gameObject;
    }

    private void SetupSpawnPositions()
    {
        for (int column = 0; column < gameLevel.Columns; column++)
        {
            SpawnPositions[column] = Match3Extensions.ComputePosition(gameLevel.Rows, column);
        }
    }

    private void AnimateGridOnPlayerScreen()
    {
        piecesParent.position = new Vector3(0, 10f, 0);
        piecesParent.DOMove(Vector3.zero, 0.5f);
    }

    #endregion

    #region Gameplay Controlls

    void Update()
    {
        if (!gridInitialized)
            return;

        if (state == GameRunningState.None)
        {
            //user has clicked or touched
            if (Input.GetMouseButtonDown(0))
            {
                //get the hit position
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null) //we have a hit!!!
                {
                    hitGo = hit.collider.gameObject;
                    var hitType = hitGo.GetComponent<Shape>().SpecialPieceType;

                    if (currentPowerUp == PowerUp.Hammer)
                        ApplyHammerPowerUp();
                    else if (!(hitType == SpecialGridPiece.BLOCK) && !(hitType == SpecialGridPiece.SHELL) && !(hitType == SpecialGridPiece.BOARD_BOMB))
                    {
                        state = currentPowerUp != PowerUp.None ? GameRunningState.ApplyingPowerUp : GameRunningState.SelectionStarted;

                        if (currentPowerUp != PowerUp.None)
                        {
                            if (currentPowerUp == PowerUp.RowColumn)
                                ApplyBeamPowerUp();

                            if (currentPowerUp == PowerUp.ColorBomb)
                                ApplyBombPowerUp();

                        }
                    }
                }
            }
        }
        else if (state == GameRunningState.SelectionStarted)
        {
            //user dragged
            if (Input.GetMouseButton(0))
            {
                var hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                //we have a hit
                if (hit.collider != null && hitGo != hit.collider.gameObject)
                {
                    if (!(hit.collider.gameObject.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.BLOCK) && !(hit.collider.gameObject.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.SHELL))
                    {
                        //user did a hit, no need to show him hints 
                        StopCheckForPotentialMatches();

                        //if the two shapes are diagonally aligned (different row and column), just return
                        if (!Match3Extensions.AreVerticalOrHorizontalNeighbors(hitGo.GetComponent<Shape>(),
                            hit.collider.gameObject.GetComponent<Shape>()))
                        {
                            state = GameRunningState.None;
                            StartCheckForPotentialMatches();
                        }
                        else
                        {
                            state = GameRunningState.Animating;
                            FixSortingLayer(hitGo, hit.collider.gameObject);
                            FindMatchesAndCollapse(hit.collider.gameObject);
                        }
                    }
                }
            }

            if (Input.GetMouseButtonUp(0) && state == GameRunningState.SelectionStarted)
            {
                state = GameRunningState.None;
            }
        }
    }

    private void ApplyBeamPowerUp()
    {
        //StopCheckForPotentialMatches();

        //int row = hitGo.GetComponent<Shape>().Row, column = hitGo.GetComponent<Shape>().Column;

        CreateBonus(hitGo.GetComponent<Shape>(), LevelsConfig.MinimumMatchesForBonus);
        RemoveFromScene(hitGo);

        currentPowerUp = PowerUp.None;
        state = GameRunningState.None;
        //StartCheckForPotentialMatches();
        //StartCoroutine(CheckAndClearMatches(Shapes.GetMatches(Shapes[row, column], true).MatchedShape, true));
    }

    private void ApplyHammerPowerUp()
    {
        StopCheckForPotentialMatches();

        if (BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo.GetComponent<Shape>().Bonus))
        {
            StartCoroutine(CheckAndClearMatches(Shapes.GetMatches(hitGo, true).MatchedShape, true));
        }
        else
        {
            StartCoroutine(CheckAndClearMatches(new List<GameObject> { hitGo }, true));
        }
    }

    private void ApplyBombPowerUp()
    {
        StopCheckForPotentialMatches();
        var BonusShape = hitGo.GetComponent<Shape>();

        if (BonusShape.SpecialPieceType == SpecialGridPiece.COLOR_BOMB)
        {
            Debug.Log("Color Bomb Already Exists In This Place");
        }
        else
        {
            CreateBonus(BonusShape, LevelsConfig.MinimumMatchesForColorBomb);
            RemoveFromScene(hitGo);
        }

        currentPowerUp = PowerUp.None;
        state = GameRunningState.None;
        StartCheckForPotentialMatches();
    }

    private void FindMatchesAndCollapse(GameObject hitGo2)
    {
        Shapes.Swap(hitGo, hitGo2);

        //move the swapped ones
        hitGo.transform.DOMove(hitGo2.transform.position, LevelsConfig.ShapesSwapDuration);
        hitGo2.transform.DOMove(hitGo.transform.position, LevelsConfig.ShapesSwapDuration);

        var allColorShapes = new List<GameObject>();
        if (hitGo.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.COLOR_BOMB)
        {
            isBombApplied = true;
            allColorShapes = Shapes.GetAllSameColorShapes(hitGo2.GetComponent<Shape>().ShapeType);
            events.LevelGameplayEvents.OnBombActivation?.Invoke(hitGo2.GetComponent<Shape>().ShapeType.ToString(), hitGo.transform.position, allColorShapes.Select(n => n.transform.position).ToList());
            allColorShapes.Add(hitGo);
            events.LevelEvents.OnSuccessfullMove?.Invoke();
            StartCoroutine(CheckAndClearMatches(allColorShapes, true));
            return;
        }

        if (hitGo2.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.COLOR_BOMB)
        {
            Debug.Log("Bomb Founds");
            isBombApplied = true;
            allColorShapes = Shapes.GetAllSameColorShapes(hitGo.GetComponent<Shape>().ShapeType);
            events.LevelGameplayEvents.OnBombActivation?.Invoke(hitGo2.GetComponent<Shape>().ShapeType.ToString(), hitGo.transform.position, allColorShapes.Select(n => n.transform.position).ToList());
            allColorShapes.Add(hitGo2);
            events.LevelEvents.OnSuccessfullMove?.Invoke();
            StartCoroutine(CheckAndClearMatches(allColorShapes, true));
            return;
        }

        //get the matches via the helper methods
        var hitGomatchesInfo = Shapes.GetMatches(hitGo);
        var hitGo2matchesInfo = Shapes.GetMatches(hitGo2);

        var totalMatches = hitGomatchesInfo.MatchedShape
            .Union(hitGo2matchesInfo.MatchedShape).Distinct();

        bool addBonus = totalMatches.Count() >= LevelsConfig.MinimumMatchesForBonus && gameLevel.CanAddBonus &&
       !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGomatchesInfo.BonusesContained) &&
       !BonusTypeUtilities.ContainsDestroyWholeRowColumn(hitGo2matchesInfo.BonusesContained);

        StartCoroutine(StartCollapsingMatches(totalMatches, hitGo2, addBonus, hitGomatchesInfo));
    }

    private IEnumerator StartCollapsingMatches(IEnumerable<GameObject> totalMatches, GameObject hitGo2 = null, bool addBonus = false, MatchesInfo hitGomatchesInfo = null)
    {
        yield return new WaitForSeconds(LevelsConfig.ShapesSwapDuration);

        if(hitGo2 != null)
        {
            //if user's swap didn't create at least a 3-match, undo their swap
            if (totalMatches.Count() < LevelsConfig.MinimumMatches)
            {
                hitGo.transform.DOMove(hitGo2.transform.position, LevelsConfig.ShapesSwapDuration);
                hitGo2.transform.DOMove(hitGo.transform.position, LevelsConfig.ShapesSwapDuration);
                yield return new WaitForSeconds(LevelsConfig.ShapesSwapDuration);
                events.LevelEvents.OnUnSuccessfullMove?.Invoke();
                Shapes.UndoSwap();
            }
            else
            {
                events.LevelEvents.OnSuccessfullMove?.Invoke();
            }
        }

        Shape hitGoCache = null;
        if (addBonus)
        {
            //get the game object that was of the same type
            var sameTypeGo = hitGomatchesInfo.MatchedShape.Count() > 0 ? hitGo : hitGo2;
            hitGoCache = sameTypeGo.GetComponent<Shape>();
        }

        StartCoroutine(CheckAndClearMatches(totalMatches, false, hitGoCache, addBonus));
    }

    private IEnumerator CheckAndClearMatches(IEnumerable<GameObject> totalMatches, bool simpleClear = false, Shape hitGoCache = null, bool addBonus = false)
    {
        if (isBombApplied)
            yield return LevelsConfig.WaitBeforeActivatingColorBomb;

        int timesRun = 1;
        List<GameObject> newTotalList = new List<GameObject>();
        List<GameObject> specialTypesNearby = new List<GameObject>();
        List<GameObject> boardBombs = new List<GameObject>(); 

        if (totalMatches.Count() == 1)
        {
            var shapeType = totalMatches.First().GetComponent<Shape>().SpecialPieceType;

            if (shapeType == SpecialGridPiece.SHELL)
            {
                specialTypesNearby.Add(totalMatches.First());
                totalMatches = new List<GameObject>();
            }

            if (shapeType == SpecialGridPiece.BLOCK)
            {
                events.LevelEvents.OnRocksDestroyed?.Invoke(1);
                specialTypesNearby.Add(totalMatches.First());
            }

            if (shapeType == SpecialGridPiece.BOARD_BOMB)
            {
                newTotalList = totalMatches.ToList();
                newTotalList.AddRange(Shapes.GetPiecesNearBoardBomb(totalMatches.First().GetComponent<Shape>()));
                specialTypesNearby.Add(totalMatches.First());
                totalMatches = newTotalList;
            }

        }

        if (totalMatches.Count() >= LevelsConfig.MinimumMatches && !isBombApplied)
        {
            if (currentPowerUp == PowerUp.None || currentPowerUp == PowerUp.RowColumn)
            {
                foreach (GameObject shape in totalMatches)
                {
                    boardBombs.AddRange(Shapes.GetSpecialTypesNearby(shape.GetComponent<Shape>(), SpecialGridPiece.BOARD_BOMB));
                }

                newTotalList = totalMatches.ToList();
                foreach (GameObject boardBomb in boardBombs)
                {
                    newTotalList.AddRange(Shapes.GetPiecesNearBoardBomb(boardBomb.GetComponent<Shape>()));
                    specialTypesNearby.Add(boardBomb);
                }

                totalMatches = newTotalList;
            }
        }

        while (totalMatches.Count() >= LevelsConfig.MinimumMatches || simpleClear)
        {
            if (CurrentPowerUp != PowerUp.Hammer)
            {
                newTotalList = totalMatches.ToList();
                for (int i = newTotalList.Count - 1; i >= 0; i--)
                {
                    GameObject shape = newTotalList[i];

                    if (shape.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.BLOCK)
                    {
                        specialTypesNearby.Add(shape);
                        newTotalList.RemoveAt(i);
                    }
                    else
                    {
                        specialTypesNearby.AddRange(Shapes.GetSpecialTypesNearby(shape.GetComponent<Shape>(), SpecialGridPiece.BLOCK));
                    }
                }

                if (specialTypesNearby.Count() > 0)
                {
                    specialTypesNearby = specialTypesNearby.Distinct(new GameObjectShapeComparer()).ToList();
                    events.LevelEvents.OnRocksDestroyed?.Invoke(specialTypesNearby.Count);
                    newTotalList.AddRange(specialTypesNearby);
                }

                specialTypesNearby = new List<GameObject>();
                for (int i = newTotalList.Count - 1; i >= 0; i--)
                {
                    GameObject shape = newTotalList[i];

                    if (shape.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.SHELL)
                    {
                        specialTypesNearby.Add(shape);
                        newTotalList.RemoveAt(i);
                    }
                    else
                    {
                        specialTypesNearby.AddRange(Shapes.GetSpecialTypesNearby(shape.GetComponent<Shape>(), SpecialGridPiece.SHELL));
                    }
                }

                totalMatches = newTotalList;
            }

            if (Shapes.Stats.horizontalBonusRow != -1)
            {
                List<Vector3> rowShapesPosList = Shapes.GetRow(Shapes.Stats.horizontalBonusRow).Select(n => n.transform.position).ToList();
                events.LevelGameplayEvents.OnRowDestroyed?.Invoke(Shapes.Stats.horizontalBonusRow, rowShapesPosList[0], rowShapesPosList[rowShapesPosList.Count - 1]);
            }

            if (Shapes.Stats.verticalBonusColumn != -1)
            {
                List<Vector3> columnShapesPosList = Shapes.GetColumn(Shapes.Stats.verticalBonusColumn).Select(n => n.transform.position).ToList();
                events.LevelGameplayEvents.OnColumnDestroyed?.Invoke(Shapes.Stats.verticalBonusColumn, columnShapesPosList[0], columnShapesPosList[columnShapesPosList.Count - 1]);
            }

            Shapes.Stats.Reset();
            yield return new WaitForSeconds(LevelsConfig.WaitBeforeCollapsing);

            totalMatches.Select(match => match.GetComponent<Shape>().ShapeType.ToString())
            .GroupBy(shape => shape)
            .ToList()
            .ForEach(group => events.LevelEvents.OnShapesMatched?.Invoke(group.Key, group.Count()));
            var shapeList = totalMatches.Select(match => match.GetComponent<Shape>().ShapeType.ToString()).ToList();

            //events.LevelEvents.OnShapesMatched?.Invoke(shapeList);

            events.LevelEvents.OnColorMatchOccurred?.Invoke(totalMatches
            .Where(shapeObj => !(shapeObj.GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.BLOCK))
            .GroupBy(shapeObj => shapeObj.GetComponent<Shape>().ShapeType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList().Select(n => n.ToString()).ToList());

            foreach (var item in totalMatches)
            {
                Shapes.Remove(item);
                RemoveFromScene(item);
            }
             
            if (specialTypesNearby.Count != 0)
            {
                specialTypesNearby = specialTypesNearby.Distinct(new GameObjectShapeComparer()).ToList();
                foreach (GameObject specialShape in specialTypesNearby)
                {
                    var shape = specialShape.GetComponent<Shape>();
                    events.LevelGameplayEvents.OnShapeDestroyed?.Invoke(shape.SpecialPieceType.ToString(), specialShape.transform.position);
                    shape.RemoveShell();
                    events.LevelEvents.OnShellsDestroyed?.Invoke(1);
                }
            }
            
            if (isBombApplied)
            {
                isBombApplied = false;
                events.LevelEvents.OnBombActivated?.Invoke();
            }

            yield return new WaitForSeconds(LevelsConfig.WaitForCollapsing);

            //check and instantiate Bonus if needed
            if (addBonus)
                CreateBonus(hitGoCache, totalMatches.Count());

            addBonus = false;

            //get the columns that we had a collapse
            var columns = totalMatches.Select(go => go.GetComponent<Shape>().Column).Distinct();

            //the order the 2 methods below get called is important!!!
            //collapse the ones gone
            var collapsedShapeInfo = Shapes.Collapse(columns);

            //create new ones
            var newShapeInfo = CreateNewShapeInSpecificColumns(columns);

            foreach (var item in newShapeInfo.ModShape)
                item.SetActive(false);
            MoveAndAnimate(collapsedShapeInfo.ModShape);

            foreach (var item in newShapeInfo.ModShape)
                item.SetActive(true);
            MoveAndAnimate(newShapeInfo.ModShape);

            //will wait for both of the above animations
            yield return new WaitForSeconds(LevelsConfig.WaitAfterCollapsing);

            //search if there are matches with the new/collapsed items
            totalMatches = Shapes.GetMatches(collapsedShapeInfo.ModShape).
                Union(Shapes.GetMatches(newShapeInfo.ModShape)).Distinct().Union(Shapes.GetMatches(specialTypesNearby));

            specialTypesNearby.Clear();

            timesRun++;
            simpleClear = false;
        }

        isBombApplied = false;
        currentPowerUp = PowerUp.None;
        state = GameRunningState.None;
        StartCheckForPotentialMatches();
    }

    private void CreateBonus(Shape hitGoCache, int piecesCount)
    {
        Vector2 position = Match3Extensions.ComputePosition(hitGoCache.Row, hitGoCache.Column);

        GameObject Bonus = Instantiate(GetBonusFromType(hitGoCache.ShapeType), position, Quaternion.identity) as GameObject;

        Bonus.transform.parent = piecesParent;
        Bonus.transform.localScale = gameLevel.CellSize * gameLevel.ShapeSizeFactor;
        Shapes[hitGoCache.Row, hitGoCache.Column] = Bonus;
        var BonusShape = Bonus.GetComponent<Shape>();
        BonusShape.Assign(hitGoCache.Type, hitGoCache.Row, hitGoCache.Column);

        if (piecesCount == LevelsConfig.MinimumMatchesForBonus)
            BonusShape.Bonus |= BonusType.DestroyWholeRowColumn;

        if (piecesCount >= LevelsConfig.MinimumMatchesForColorBomb)
        {
            BonusShape.SpecialPieceType = SpecialGridPiece.COLOR_BOMB;
            BonusShape.SetSprite(LevelsConfig.GetSpecialPieceInfo(SpecialGridPiece.COLOR_BOMB).DisplaySprite);
        }
    }


    private void StartCheckForPotentialMatches()
    {
        StopCheckForPotentialMatches();

        CheckPotentialMatchesCoroutine = CheckPotentialMatches();
        StartCoroutine(CheckPotentialMatchesCoroutine);
    }

    private void StopCheckForPotentialMatches()
    {
        if (CheckPotentialMatchesCoroutine != null)
            StopCoroutine(CheckPotentialMatchesCoroutine);

        Shapes.isPotentialMatchSearch = false;
        ResetPotentialMatchesAnimation();
    }

    private IEnumerator CheckPotentialMatches()
    {
        yield return new WaitForSeconds(LevelsConfig.WaitForPotentialMatch);

        Shapes.isPotentialMatchSearch = true;
        potentialMatches = Match3Extensions.GetPotentialMatches(Shapes);
        Shapes.isPotentialMatchSearch = false;
        if (potentialMatches != null)
        {
            while (true && gameLevel.CanShowHint)
            {
                ResetPotentialMatchesAnimation();
                foreach (GameObject obj in potentialMatches)
                {
                    obj.transform.GetChild(0).GetComponent<Animator>().Play("Highlight Anim");
                }

                yield return new WaitForSeconds(LevelsConfig.WaitForPotentialMatch);
            }
        }
        else
        {
            events.LevelEvents.OnShapesShuffled?.Invoke();
            ShuffleShapes();
        }
    }

    #endregion

    #region Element Spawn And Remove

    private void RemoveFromScene(GameObject item)
    {
        item.SetActive(false);
        Shape shape = item.GetComponent<Shape>();
        events.LevelGameplayEvents.OnShapeDestroyed?.Invoke(shape.SpecialPieceType == SpecialGridPiece.NONE ? shape.ShapeType.ToString() : shape.SpecialPieceType.ToString(), item.transform.position);
        Destroy(item, 2f);
    }

    private ModShapeInfo CreateNewShapeInSpecificColumns(IEnumerable<int> columnsWithMissingShape)
    {
        ModShapeInfo newShapeInfo = new ModShapeInfo();

        //find how many null values the column has
        foreach (int column in columnsWithMissingShape)
        {
            var emptyItems = Shapes.GetEmptyItemsOnColumn(column);
            foreach (var item in emptyItems)
            {
                GameObject newPiece = null;
                if (Match3Extensions.CanSpawnBoardBomb())
                {
                    newPiece = InstantiateAndPlaceNewSpecialPiece(item.Row, column, GetRandomShape(), SpecialGridPiece.BOARD_BOMB);
                }
                else
                {
                    var go = GetRandomShape();
                    Vector3 piecePos = SpawnPositions[column] + new Vector2(0, Match3Extensions.ComputePosition(item.Row, item.Column).x);
                    newPiece = Instantiate(go, new Vector3(piecePos.x, LevelsConfig.ShapesFallHeight, piecePos.z) , Quaternion.identity)
                        as GameObject;

                    newPiece.transform.parent = piecesParent;
                    newPiece.transform.localScale = gameLevel.CellSize * gameLevel.ShapeSizeFactor;
                    newPiece.GetComponent<Shape>().Assign(go.GetComponent<Shape>().Type, item.Row, item.Column);
                }
                
                if (gameLevel.Rows - item.Row > newShapeInfo.MaxDistance)
                    newShapeInfo.MaxDistance = gameLevel.Rows - item.Row;

                Shapes[item.Row, item.Column] = newPiece;
                newShapeInfo.AddShape(newPiece);
            }
        }
        return newShapeInfo;
    }

    private void DestroyAllShapes()
    {
        for (int row = 0; row < gameLevel.Rows; row++)
        {
            for (int column = 0; column < gameLevel.Columns; column++)
            {
                Destroy(Shapes[row, column]);
            }
        }
    }

    public void ShuffleShapes()
    {
        if (state != GameRunningState.None)
            return;

        currentPowerUp = PowerUp.Shuffle;
        StopCheckForPotentialMatches();
        List<Shape> allShapes = new List<Shape>();

        for (int row = 0; row < gameLevel.Rows; row++)
        {
            for (int column = 0; column < gameLevel.Columns; column++)
            {
                if (Shapes[row, column] != null)
                {
                    if(Shapes[row, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                        allShapes.Add(Shapes[row, column].GetComponent<Shape>());
                }
            }
        }

        // Shuffle the list
        System.Random rng = new System.Random();
        int n = allShapes.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Shape value = allShapes[k];
            allShapes[k] = allShapes[n];
            allShapes[n] = value;
        }

        List<GameObject> shapesToMove = new List<GameObject>(); 

        int index = 0;
        for (int row = 0; row < gameLevel.Rows; row++)
        {
            for (int column = 0; column < gameLevel.Columns; column++)
            {
                if (Shapes[row, column] != null) 
                {
                    if (Shapes[row, column].GetComponent<Shape>().SpecialPieceType == SpecialGridPiece.NONE)
                    {
                        Shape shape = allShapes[index++];
                        shape.Row = row;
                        shape.Column = column;

                        shapesToMove.Add(shape.gameObject);
                        Shapes[row, column] = shape.gameObject;
                    }
                }
            }
        }

        foreach (var _shape in shapesToMove)
        {
            var matchCont = Shapes.GetMatches(_shape).MatchedShape;
            if (matchCont.Count() >= LevelsConfig.MinimumMatches)
            {
                ShuffleShapes();
                return;
            }
        }

        MoveAndAnimate(shapesToMove);
        StartCheckForPotentialMatches();

        DOVirtual.DelayedCall(LevelsConfig.ShapesCollapseDuration, () => currentPowerUp = PowerUp.None);
    }

    #endregion

    #region Utilities

    private void FixSortingLayer(GameObject hitGo, GameObject hitGo2)
    {
        SpriteRenderer sp1 = hitGo.transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer sp2 = hitGo2.transform.GetChild(0).GetComponent<SpriteRenderer>();
        if (sp1.sortingOrder <= sp2.sortingOrder)
        {
            sp1.sortingOrder = 2;
            sp2.sortingOrder = 1;
        }
    }

    private void ResetPotentialMatchesAnimation()
    {
        if (potentialMatches != null)
            foreach (var item in potentialMatches)
            {
                if (item == null) break;
                
                item.transform.GetChild(0).GetComponent<Animator>().Play("Idle");
            }
    }

    private void MoveAndAnimate(IEnumerable<GameObject> movedGameObjects)
    {
        foreach (var item in movedGameObjects)
        {
            Shape shapeComponent = item.GetComponent<Shape>();
            Vector2 targetPosition = Match3Extensions.ComputePosition(shapeComponent.Row, shapeComponent.Column);

            //item.transform.position = new Vector3(item.transform.position.x, levelsConfig.ShapesFallHeight, item.transform.position.z);

            float distance = Vector2.Distance(item.transform.position, targetPosition);
            float minDuration = 0.1f;
            float maxDuration = LevelsConfig.ShapesCollapseDuration;
            float duration = Mathf.Clamp(distance / LevelsConfig.CollapseSpeedFactor, minDuration, maxDuration);
            item.transform.DOMove(targetPosition, duration);
        }
    }

    private GameObject GetRandomShape()
    {
        return shapesDict[new List<ShapeType>(shapesDict.Keys)[Random.Range(0, shapesDict.Count)]].shapePrefab;
    }

    private GameObject GetRandomShapeForRolColumn(int row, int column)
    {
        GameObject newShape = GetRandomShape();

        if (gameLevel.RandomizeColorShapes)
        {
            while (column >= 2 && Shapes.IsSame(Shapes[row, column - 1], newShape)
                && Shapes.IsSame(Shapes[row, column - 2], newShape))
            {
                newShape = GetRandomShape();
            }

            while (row >= 2 && Shapes.IsSame(Shapes[row - 1, column], newShape)
                && Shapes.IsSame(Shapes[row - 2, column], newShape))
            {
                newShape = GetRandomShape();
            }
        }

        return newShape;
    }

    private GameObject GetShape(ShapeTypeEditor type)
    {
        return shapesDict[Match3Extensions.GetShapeType(type)].shapePrefab;
    }

    private GameObject GetBonusFromType(ShapeType type)
    {
        return shapesDict[type].bonusPrefab;
    }

    #endregion

}