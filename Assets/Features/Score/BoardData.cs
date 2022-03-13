using System;
using UnityEngine;

[CreateAssetMenu]
public class BoardData : ScriptableObject, ISerializationCallbackReceiver
{
    public event Action OnScoreChanged;
    public event Action OnLevelChanged;
    public event Action OnBoardClearEvent;
    public event Action OnGameStop;
    public event Action OnGameStart;

    public int Score;
    public int Level;
    public int LinesCleared;

    public float BlockStepDuration => Mathf.Pow(1f / 1.5f, Level - 1) * 5;


    public void ApplyPointsForLines(int lines)
    {
        ApplyLinesForLevel(lines);
        int baseValue = 0;
        switch (lines)
        {
            case 1:
                baseValue = 40;
                break;
            case 2:
                baseValue = 100;
                break;
            case 3:
                baseValue = 300;
                break;
            case 4:
                baseValue = 1200;
                break;
        }

        Score += baseValue * (Level + 1);
        OnScoreChanged?.Invoke();
    }

    private void ApplyLinesForLevel(int lines)
    {
        int oldMod = LinesCleared % 10;
        LinesCleared += lines;
        int newMpd = LinesCleared % 10;
        if (newMpd < oldMod)
        {
            IncreaseLevel();
        }
    }

    private void IncreaseLevel()
    {
        Level++;
        OnLevelChanged?.Invoke();
    }

    public void ClearBoard()
    {
        Score = 0;
        Level = 1;
        LinesCleared = 0;
        OnScoreChanged?.Invoke();
        OnBoardClearEvent?.Invoke();
        OnLevelChanged?.Invoke();
        OnGameStart?.Invoke();
    }

    public void OnBeforeSerialize()
    {
    }

    public void OnAfterDeserialize()
    {
        Score = 0;
        Level = 1;
        LinesCleared = 0;
    }

    public void StopGame()
    {
        OnGameStop?.Invoke();
    }
}