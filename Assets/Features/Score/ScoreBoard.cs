using TMPro;
using UnityEngine;

public class ScoreBoard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _score;
    [SerializeField] private TextMeshProUGUI _level;
    [SerializeField] private TextMeshProUGUI _linesCleard;
    
    [SerializeField] private BoardData _boardData;

    private void Start()
    {
        _boardData.OnScoreChanged += UpdateScore;
        _boardData.OnLevelChanged += UpdateLevel;
        _boardData.OnBoardClearEvent += ClearScore;
        ClearScore();
    }

    private void ClearScore()
    {
        UpdateScore();
        UpdateLevel();
    }

    private void UpdateLevel()
    {
        _level.text = $"Level: {_boardData.Level}";
    }

    private void UpdateScore()
    {
        _score.text = $"Score: {_boardData.Score}";
        _linesCleard.text = $"Lines Cleared: {_boardData.LinesCleared}";
    }
}
