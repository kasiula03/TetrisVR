using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class BoardController : MonoBehaviour
{
    [SerializeField] private BoardData _boardData;
    [SerializeField] private BlockSpawner _blockSpawner;
    [SerializeField] private Transform _segmentTransform;
    [SerializeField] private Transform _floorPoint;
    [SerializeField] private GridHightlight _gridHightlight;
    [SerializeField] private ParticleSystem _lineEffect;


    [Inject] private TimersManager _timersManager;

    private float _scaleDownValueX => transform.lossyScale.x;
    private float _scaleDownValueY => transform.lossyScale.y;
    private float _scaleXOffset = 0.5f;

    private const int Width = 10;
    private const int Height = 20;
    private const int HalfWidth = Width / 2;

    private Vector3 SpawnPosition => new Vector3(HalfWidth, Height, 0);

    private BoardState _boardState;

    private TimerAction _timer;

    private Block _currentBlock;
    private Vector2 _currentPosition;

    private void Start()
    {
        _boardState = new BoardState(_boardData, Width, Height);
        _blockSpawner.Setup(BlockMoved, MoveToTheBottom, MoveBlockDown, RotateBlock);
        UpdateGridHint();

        _timer = new TimerAction(_boardData.BlockStepDuration, MoveBlockDown);

        _boardData.OnBoardClearEvent += ClearBoard;
        _boardData.OnLevelChanged += ResetTimer;
        _boardData.OnGameStop += StopGame;
        _boardData.OnGameStart += StartGame;
    }

    private void StartGame()
    {
        MoveBlockDown();
    }

    private void StopGame()
    {
        _timersManager.Unsubscribe(_timer);
    }

    private void ResetTimer()
    {
        _timersManager.Unsubscribe(_timer);
        _timer = new TimerAction(_boardData.BlockStepDuration, MoveBlockDown);
        _timersManager.Subscribe(_timer, false);
    }
    
    private void RotateBlock(Block block, int angleToRotate)
    {
        if (_currentBlock != block)
        {
            return;
        }

        if (IsRotationValid(block, _currentPosition, angleToRotate))
        {
            block.Rotate(angleToRotate);
            UpdateGridHint();
        }
    }

    private void MoveBlockDown(Block block, Vector2 offset)
    {
        Vector2 currentPosition = GetCurrentPosition(block);
        if (CanMove(block, currentPosition, offset))
        {
            block.MoveTo(GetGlobalPosition(currentPosition + offset), offset, true);
        }
    }

    private void MoveToTheBottom(Block block)
    {
        if (_currentBlock != block)
        {
            return;
        }

        MoveToTheBottom(block, GetCurrentPosition(block));
    }

    private void BlockMoved(Vector2 offset)
    {
        _currentPosition += offset;
        UpdateGridHint();
    }

    private Vector2 GetCurrentPosition(Block block)
    {
        if (_currentBlock == block)
        {
            return _currentPosition;
        }
        else
        {
            Debug.LogWarning("Getting position for not active block");
            return Vector2.zero;
        }
    }

    private void UpdateGridHint()
    {
        IEnumerable<Vector2> segmentsPositions = new List<Vector2>();
        Vector2 position = Vector2.zero;
        float width = 0;
        int minY = 0;
        Color blockColor = Color.white;
        if (_currentBlock)
        {
            int minBlockX = (int) _currentBlock.GetMinX(_currentPosition.x);
            int maxBlockX = (int) _currentBlock.GetMaxX(_currentPosition.x);

            Vector2 maxYOffset = new Vector2(0, GetYSteps(_currentBlock, _currentPosition));
            Vector2 offsetPosition = _currentPosition + maxYOffset;
            segmentsPositions = _currentBlock.GetSegments().Select(seg => seg + offsetPosition);

            position = new Vector2(_currentBlock.GetMinX(_currentPosition.x),
                _currentBlock.GetMinY(_currentPosition.y));
            width = _currentBlock.GetWidth();
            minY = _boardState.GetMaxOccupiedY(minBlockX, maxBlockX);
            blockColor = _currentBlock.GetColor();
        }


        _gridHightlight.UpdateGrid(position, width, minY, segmentsPositions.ToArray(), blockColor);
    }

    private int GetYSteps(Block block, Vector2 currentPosition)
    {
        int yOffset = 0;

        while (CanMove(block, currentPosition, new Vector2(0, yOffset)))
        {
            yOffset -= 1;
        }

        if (yOffset == 0)
        {
            return yOffset;
        }

        return yOffset + 1;
    }

    private bool CanMove(Block block, Vector2 position, Vector2 offset)
    {
        Vector2 offsetPosition = position + offset;

        if (block.GetMinY(offsetPosition.y) < 0 && Math.Abs(offset.y) > 0.0001f)
        {
            return false;
        }

        float minX = block.GetMinX(offsetPosition.x);
        float maxX = block.GetMaxX(offsetPosition.x);

        bool touchLeftBorder = minX < 0 || maxX < 0;
        bool touchRightBorder = minX >= Width || maxX >= Width;

        if (Math.Abs(offset.x) > 0.0001f && (touchLeftBorder || touchRightBorder))
        {
            return false;
        }

        foreach (Vector2 segmentLocalPosition in block.GetSegments())
        {
            Vector2 positionOnGrid = segmentLocalPosition + offsetPosition;
            int xIndex = (int) positionOnGrid.x;
            int yIndex = (int) positionOnGrid.y;

            if (xIndex < 0 || xIndex > Width - 1 || yIndex < 0 || yIndex > Height - 1)
            {
                continue;
            }

            if (_boardState.IsOccupied(xIndex, yIndex))
            {
                return false;
            }
        }

        return true;
    }

    public bool IsRotationValid(Block block, Vector2 position, float rotation)
    {
        BlockProperties predictedProperties = block.GetPropertiesCopy();
        predictedProperties.AddRotation(rotation);

        float xOriginPos = position.x;

        float minY = block.GetPredictMinY(position.y, predictedProperties);

        if (minY < 0)
        {
            return false;
        }

        float minX = block.GetPredictMinX(xOriginPos, predictedProperties);
        float maxX = block.GetPredictMaxX(xOriginPos, predictedProperties);

        bool touchLeftBorder = minX < 0 || maxX < 0;
        bool touchRightBorder = minX > Width - 1 || maxX > Width - 1;

        if (touchLeftBorder || touchRightBorder)
        {
            return false;
        }

        foreach (Vector2 segmentLocalPosition in block.GetSegments(predictedProperties))
        {
            Vector2 positionOnGrid = segmentLocalPosition + position;
            int xIndex = (int) positionOnGrid.x;
            int yIndex = (int) positionOnGrid.y;

            if (xIndex < 0 || xIndex > Width - 1 || yIndex < 0 || yIndex > Height - 1)
            {
                continue;
            }

            if (_boardState.IsOccupied(xIndex, yIndex))
            {
                return false;
            }
        }

        return true;
    }

    private void PlaceCurrentBlockOnGrid()
    {
        Block block = _currentBlock;
        Vector2 position = _currentPosition;

        ResetCurrentBlock();
        _boardState.PlaceOnGrid(block, position);
        foreach (Transform segment in block.Segments.Values)
        {
            segment.SetParent(_segmentTransform);
        }
    }

    private void ClearBoard()
    {
        ResetCurrentBlock();
        for (int i = 0; i < Width; i++)
        {
            for (int j = 0; j < Height; j++)
            {
                Transform segment = _boardState.GetSegment(i, j);
                if (segment != null)
                {
                    Destroy(segment.gameObject);
                }
            }
        }

        _boardState.ClearBoard();
        _gridHightlight.Clear();
    }

    private void ResetCurrentBlock()
    {
        if (_currentBlock)
        {
            _currentBlock.Release();
            Destroy(_currentBlock.gameObject);
        }

        _currentBlock = null;
        _currentPosition = Vector2.negativeInfinity;
    }

    private void MoveBlockDown()
    {
        Vector2 offset = new Vector2(0, -1);
        if (_currentBlock && CanMove(_currentBlock, _currentPosition, new Vector2(0, -1)))
        {
            _currentBlock.MoveTo(GetGlobalPosition(_currentPosition + offset), offset, false);
        }
        else if (!IsAnyAnimationRunning())
        {
            if (_currentBlock)
            {
                PlaceCurrentBlockOnGrid();
            }

            EndTurn();
        }
    }

    private void MoveToTheBottom(Block block, Vector2 position)
    {
        Vector2 offset = new Vector3(0, GetYSteps(block, position));
        Vector2 bottomPosition = position + offset;
        _currentBlock.MoveTo(GetGlobalPosition(bottomPosition), offset, false);
        PlaceCurrentBlockOnGrid(block, bottomPosition);
        EndTurn();
        UpdateGridHint();
    }

    private void PlaceCurrentBlockOnGrid(Block block, Vector2 bottomPosition)
    {
        PlaceCurrentBlockOnGrid();
    }


    private Sequence _removeRowAnim;
    private Sequence _gameOverAnim;

    private bool IsAnyAnimationRunning()
    {
        return (_removeRowAnim?.IsPlaying() ?? false) || (_gameOverAnim?.IsPlaying() ?? false);
    }

    private async Task EndTurn()
    {
        Debug.Log("End turn!");
        await CheckTetris();
        Block block = _blockSpawner.SpawnBlock(SpawnPosition);
        BlockEnter(block);
        UpdateGridHint();
        CheckGameOver(block);
    }

    private async Task CheckTetris()
    {
        _removeRowAnim?.Kill();
        _removeRowAnim = DOTween.Sequence();

        List<int> linesToClear = _boardState.CheckTetris();
        if (linesToClear.Count == 0)
        {
            return;
        }

        int lineToClearAmount = linesToClear.Count;

        foreach (int rowIndex in linesToClear)
        {
            List<Transform> segments = _boardState.GetSegmentsOnRow(rowIndex);
            Sequence sequence = BoardAnimations.PlayDissolveEffect(segments, lineToClearAmount, rowIndex, _lineEffect);
            _removeRowAnim.Append(sequence);
        }

        await _removeRowAnim.AsyncWaitForCompletion();

        _boardData.ApplyPointsForLines(lineToClearAmount);

        foreach (int rowIndex in linesToClear)
        {
            for (int x = 0; x < Width; x++)
            {
                var xIndex = x;
                Transform segment = _boardState.GetSegment(xIndex, rowIndex);

                if (segment)
                {
                    Destroy(segment.gameObject);
                    _boardState.RemoveFromGrid(xIndex, rowIndex);
                }
            }
        }

        for (int i = 0; i < linesToClear.Count; i++)
        {
            int lineIndex = linesToClear[i] - i;
            MoveAllAboveDown(lineIndex);
        }
    }

    private void MoveAllAboveDown(int rowIndex)
    {
        for (int index = rowIndex + 1; index < Height; index++)
        {
            _boardState.MoveRowDown(index);
        }
    }


    private void BlockEnter(Block block)
    {
        if (_currentBlock)
        {
            Debug.LogError("");
        }

        _currentBlock = block;
        _currentPosition = SpawnPosition;
        _currentBlock.transform.position = GetGlobalPosition(_currentPosition);
    }

    private void CheckGameOver(Block block)
    {
        if (!CanMove(block, _currentPosition, new Vector2(0, -1)))
        {
            _boardData.StopGame();
            PlayStoningSegmentsEffect();
            block.Release();
            Debug.Log("Game over!!");
        }
    }

    private void PlayStoningSegmentsEffect()
    {
        List<Transform> segments = _segmentTransform.GetComponentsInChildren<Transform>().ToList();
        segments.AddRange(_currentBlock.Segments.Values);

        _gameOverAnim?.Kill(true);
        _gameOverAnim = BoardAnimations.PlayStoningSegmentsEffect(segments);
    }

    private Vector3 GetGlobalPosition(Vector2 onBoardPosition)
    {
        return _floorPoint.position +
               new Vector3((onBoardPosition.x - HalfWidth + _scaleXOffset) * _scaleDownValueX,
                   onBoardPosition.y * _scaleDownValueY, 0);
    }
}