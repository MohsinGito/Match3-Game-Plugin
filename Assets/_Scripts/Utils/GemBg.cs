using UnityEngine;

public class GemBg : MonoBehaviour
{

    public GameObject LeftFrame;
    public GameObject RightFrame;
    public GameObject TopFrame;
    public GameObject BottomFrame;

    public void SetFrame(int _row, int _column, GameObject[,] _bgArr, GameLevel _level)
    {
        if (_row == 0 || _bgArr[_row, _column] == null)
        {
            BottomFrame.SetActive(true);
        }
        if (_column == _level.Columns - 1 || _bgArr[_row, _column] == null)
        {
            RightFrame.SetActive(true);
        }
        if (_row == _level.Rows - 1 || _bgArr[_row, _column] == null)
        {
            TopFrame.SetActive(true);
        }
        if (_column == 0 || _bgArr[_row, _column] == null)
        {
            LeftFrame.SetActive(true);
        }

    }

}
