using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class BlockProperties
{
    public enum BlockType
    {
        None,
        I,
        J,
        L,
        O,
        S,
        T,
        Z,
    }

    public readonly BlockType Type;

    public readonly ReadOnlyCollection<Vector2> SegmentsCoords;
    private float Rotation = 0;
    
    private Vector2 Origin = Vector2.zero;

    public BlockProperties(BlockType type)
    {
        Type = type;
        Profiler.BeginSample("Create a segment");
        SegmentsCoords = BlockTypeSegments.GetSegments(type);
        Profiler.EndSample();
    }

    public BlockProperties Copy()
    {
        return (BlockProperties) MemberwiseClone();
    }
    
    public float GetRotation()
    {
        return Rotation;
    }

    public void AddRotation(float rotation)
    {
        Rotation += rotation;
    }

    public Vector2 GetRotatedSegment(Vector2 segment)
    {
        return RotateAroundPivot(segment, Origin, new Vector3(0, 0, Rotation));
    }

    public List<Vector2> GetSegments()
    {
        Vector2 origin = Origin;
        return SegmentsCoords
            .Select(cords => RotateAroundPivot(cords, origin, new Vector3(0, 0, Rotation))).ToList();
    }

    public List<Vector2> GetSegments(BlockProperties properties)
    {
        Vector2 origin = properties.Origin;
        return properties.SegmentsCoords
            .Select(cords => RotateAroundPivot(cords, origin, new Vector3(0, 0, properties.Rotation))).ToList();
    }

    private Vector2 RotateAroundPivot(Vector2 point, Vector2 pivot, Vector3 angles)
    {
        Vector2 dir = point - pivot;
        Vector3 rotation = Quaternion.Euler(angles) * dir;
        return new Vector2((float) Math.Round(rotation.x), (float) Math.Round(rotation.y)) + pivot;
    }

    public float GetMinX(float originX)
    {
        return originX + GetSegments().Min(segment => segment.x);
    }

    public float GetPredictMinX(float originX, BlockProperties predictedProperties)
    {
        return originX + GetSegments(predictedProperties).Min(segment => segment.x);
    }

    public float GetMaxX(float originX)
    {
        return originX + GetSegments().Max(segment => segment.x);
    }

    public float GetPredictMaxX(float originX, BlockProperties predictedProperties)
    {
        return originX + GetSegments(predictedProperties).Max(segment => segment.x);
    }

    public float GetMinY(float originY)
    {
        return originY + GetSegments().Min(segment => segment.y);
    }

    public float GetPredictMinY(float originY, BlockProperties predictedProperties)
    {
        return originY + GetSegments(predictedProperties).Min(segment => segment.y);
    }

    public float GetWidth()
    {
        return GetMaxX(0) - GetMinX(0) + 1;
    }
}