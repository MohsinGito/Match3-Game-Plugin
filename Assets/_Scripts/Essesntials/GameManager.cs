using UnityEngine;

public class GameManager : MonoBehaviour
{

    public PlayerData playerData;
    public Match3Manager match3Manager;

    private void Start()
    {
        match3Manager.StartLevel(playerData.CurrentLevel);
    }

    public void OnLevelStart(GameLevel _level)
    {
        for (int i = 0; i < _level.Rows; i++)
        {
            for (int j = 0; j < _level.Columns; j++)
            {
                if (match3Manager.Shapes.ShapesBgs[i, j] == null) continue;
                match3Manager.Shapes.ShapesBgs[i, j].GetComponent<GemBg>().SetFrame(i, j, match3Manager.Shapes.ShapesBgs, _level);
            }
        }
    }

    public void OnShapeDestroyed(string _shapeName, Vector3 _destroyedAt)
    {
        if (!_shapeName.Equals("JELLY"))
            return;

        Debug.Log("Jelly Destroyed At : " + _destroyedAt);
    }

}