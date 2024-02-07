using UnityEngine;

[System.Serializable]
public class Grid2DLayout
{

    [System.Serializable]
    public struct rowData
    {
        public bool[] row;
    }

    public rowData[] rows;
}