using System;
using UnityEngine;

public class BlockEffect : MonoBehaviour
{
    [SerializeField] private MeshRenderer _meshRenderer;

    private MaterialPropertyBlock _propertyBlock;
    private static readonly int DisplacementY = Shader.PropertyToID("_DisplacementY");
    private static readonly int DisplacementX = Shader.PropertyToID("_DisplacementX");
    private static readonly int Rotation = Shader.PropertyToID("_Rotation");
    private static readonly int WorldScaleInversed = Shader.PropertyToID("_WorldScaleInversed");

    private void Start()
    {
        Vector3 objectScale = transform.lossyScale;
        _propertyBlock = new MaterialPropertyBlock();
        _propertyBlock.SetVector(WorldScaleInversed,
            new Vector3(1 / objectScale.x, 1 / objectScale.y, 1 / objectScale.z));
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetDisplacementY(float percent)
    {
        _propertyBlock.SetFloat(DisplacementY, percent);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetDisplacementX(float percent)
    {
        _propertyBlock.SetFloat(DisplacementX, percent);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }

    public void SetRotation(float degree)
    {
        _propertyBlock.SetFloat(Rotation, degree);
        _meshRenderer.SetPropertyBlock(_propertyBlock);
    }
}