using System.Collections.Generic;
using UnityEngine;

public class BoardState
{
    private readonly BoardData _boardData;
    private readonly Transform[,] _gridsOfBlockSegment;

    private readonly int _width;
    private readonly int _height;

    public Transform GetSegment(int x, int y) => _gridsOfBlockSegment[x, y];
    
    public BoardState(BoardData boardData, int width, int height)
    {
        _width = width;
        _height = height;
        _boardData = boardData;
        _gridsOfBlockSegment = new Transform[width, height];
    }

    public bool IsOccupied(int xIndex, int yIndex)
    {
        return _gridsOfBlockSegment[xIndex, yIndex] != null;
    }

    public void PlaceOnGrid(Block block, Vector2 position)
    {
        foreach (KeyValuePair<Vector2, Transform> blockSegment in block.Segments)
        {
            Transform segmentTransform = blockSegment.Value;
            Vector2 segmentLocalPosition = block.GetRotatedSegment(blockSegment.Key);
            Vector2 positionOnGrid = segmentLocalPosition + position;
            int xIndex = (int) positionOnGrid.x;
            int yIndex = (int) positionOnGrid.y;

            if (xIndex < 0 || xIndex > _width - 1 || yIndex < 0 || yIndex > _height - 1)
            {
                Debug.LogError("Wrong index!");
            }
            else
            {
                _gridsOfBlockSegment[xIndex, yIndex] = segmentTransform;
            }
        }
    }

    public void RemoveFromGrid(int x, int y)
    {
        Transform segment = _gridsOfBlockSegment[x, y];

        if (segment)
        {
            _gridsOfBlockSegment[x, y] = null;
        }
    }

    public void ClearBoard()
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                _gridsOfBlockSegment[i, j] = null;
            }
        }
    }
    
    public void MoveRowDown(int rowIndex)
    {
        if (rowIndex < 1)
        {
            return;
        }

        for (int x = 0; x < _width; x++)
        {
            Transform segment = _gridsOfBlockSegment[x, rowIndex];
            _gridsOfBlockSegment[x, rowIndex] = null;
            if (segment != null)
            {
                segment.localPosition =
                    new Vector3(segment.localPosition.x, segment.localPosition.y - 1 * segment.localScale.y,
                        segment.localPosition.z);
            }

            _gridsOfBlockSegment[x, rowIndex - 1] = segment;
        }
    }

    public int GetMaxOccupiedY(int minBlockX, int maxBlockX)
    {
        int maxY = 0;
        for (int y = 0; y < _height; y++)
        {
            for (int x = minBlockX; x <= maxBlockX; x++)
            {
                if (_gridsOfBlockSegment[x, y] != null)
                {
                    maxY = y + 1;
                }
            }
        }

        return maxY;
    }

    public List<int> CheckTetris()
    {
        List<int> rowsToClear = new List<int>();
        int lineToClearAmount = 0;
        int startingIndex = -1;
        for (int y = 0; y < _height; y++)
        {
            int segmentsInRow = 0;
            for (int x = 0; x < _width; x++)
            {
                if (_gridsOfBlockSegment[x, y] != null)
                {
                    segmentsInRow++;
                }
            }

            if (segmentsInRow == _width)
            {
                lineToClearAmount++;
                rowsToClear.Add(y);
            }
        }

        return rowsToClear;
    }

    public List<Transform> GetAllSegments()
    {
        List<Transform> segments = new List<Transform>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                Transform segment = _gridsOfBlockSegment[x, y];
                if (segment)
                {
                    segments.Add(segment);
                }
            }
        }

        return segments;
    }

    public List<Transform> GetSegmentsOnRow(int rowIndex)
    {
        List<Transform> segments = new List<Transform>();
        for (int x = 0; x < _width; x++)
        {
            Transform segment = _gridsOfBlockSegment[x, rowIndex];
            if (segment)
            {
                segments.Add(segment);
            }
        }

        return segments;
    }
}