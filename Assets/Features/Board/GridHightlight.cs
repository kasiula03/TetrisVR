using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridHightlight : MonoBehaviour
{
    [SerializeField] private MeshRenderer _frontRenderer;
    [SerializeField] private MeshRenderer _leftRenderer;
    [SerializeField] private MeshRenderer _rightRenderer;
    [SerializeField] private MeshRenderer _upRenderer;

    private MaterialPropertyBlock _propertyBlock;

    private Vector3 _leftStartPosition;
    private Vector3 _rightStartPosition;
    private Vector3 _upStartPosition;

    private static readonly int Position = Shader.PropertyToID("_Position");
    private static readonly int Width = Shader.PropertyToID("_Width");
    private static readonly int MinY = Shader.PropertyToID("_MinY");
    private static readonly int FirstSegmentPosition = Shader.PropertyToID("_FirstSegmentPosition");
    private static readonly int SecondSegmentPosition = Shader.PropertyToID("_SecondSegmentPosition");
    private static readonly int ThirdSegmentPosition = Shader.PropertyToID("_ThirdSegmentPosition");
    private static readonly int FourthSegmentPosition = Shader.PropertyToID("_FourthSegmentPosition");
    private static readonly int BlockColor = Shader.PropertyToID("_BlockColor");

    private void Awake()
    {
        _leftStartPosition = _leftRenderer.transform.localPosition;
        _rightStartPosition = _rightRenderer.transform.localPosition;
        _upStartPosition = _upRenderer.transform.localPosition;
    }

    public void UpdateGrid(Vector2 position, float width, int minY, Vector2[] segmentsPositions, Color blockColor)
    {
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetVector(Position, position);
        _propertyBlock.SetFloat(Width, width);
        _propertyBlock.SetFloat(MinY, minY);

        if (segmentsPositions.Length == 4)
        {
            _propertyBlock.SetVector(FirstSegmentPosition, segmentsPositions[0]);
            _propertyBlock.SetVector(SecondSegmentPosition, segmentsPositions[1]);
            _propertyBlock.SetVector(ThirdSegmentPosition, segmentsPositions[2]);
            _propertyBlock.SetVector(FourthSegmentPosition, segmentsPositions[3]);
            _propertyBlock.SetColor(BlockColor, blockColor);
        }

        _frontRenderer.SetPropertyBlock(_propertyBlock);
        _upRenderer.SetPropertyBlock(_propertyBlock);

        _propertyBlock.SetVector(Position, new Vector4(0, position.y));
        _leftRenderer.SetPropertyBlock(_propertyBlock);
        _rightRenderer.SetPropertyBlock(_propertyBlock);

        float leftX = position.x * 0.1f;
        _leftRenderer.transform.localPosition = _leftStartPosition + new Vector3(leftX, 0, 0);

        float rightX = (position.x + width) * 0.1f;
        _rightRenderer.transform.localPosition = _rightStartPosition - new Vector3(1 - rightX, 0, 0);

        float upY = (20 - position.y) * 0.1f;
        _upRenderer.transform.localPosition = _upStartPosition - new Vector3(0, upY / 2, 0);
    }

    public void Clear()
    {
        UpdateGrid(Vector2.zero, 0, 0, Array.Empty<Vector2>(), Color.black);
    }
}