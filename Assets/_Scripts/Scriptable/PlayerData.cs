using UnityEngine;

[CreateAssetMenu( fileName = "Player Data", menuName = "Player Data")]
public class PlayerData : ScriptableObject
{

    public int TotalScores;
    public int TotalCoins;
    public int CurrentLevel;
    public int HammerPowerUpCount;
    public int RowDestroyPowerUpCount;
    public int ColorBombPowerUpCount;
    public int ShufflePowerupCount;

    public void AddPowerup(PowerUp powerUp, int count)
    {
        switch (powerUp)
        {
            case PowerUp.Hammer:
                HammerPowerUpCount += count;
                break;
            case PowerUp.RowColumn:
                RowDestroyPowerUpCount += count;
                break;
            case PowerUp.ColorBomb:
                ColorBombPowerUpCount += count;
                break;
            case PowerUp.Shuffle:
                ShufflePowerupCount += count;
                break;
        }
    }


}