using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public static class BlockTypeSegments
{
    private static readonly ReadOnlyCollection<Vector2> ISegments = new List<Vector2>()
    {
        new Vector2(0f, 0),
        new Vector2(1f, 0),
        new Vector2(2f, 0),
        new Vector2(3f, 0)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> JSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(2, 0),
        new Vector2(0, 1)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> LSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(2, 0),
        new Vector2(2, 1)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> OSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(1, 1)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> SSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(1, 1),
        new Vector2(2, 1)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> TSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(0, 1),
        new Vector2(-1, 1),
        new Vector2(1, 1)
    }.AsReadOnly();

    private static readonly ReadOnlyCollection<Vector2> ZSegments = new List<Vector2>()
    {
        new Vector2(0, 0),
        new Vector2(1, 0),
        new Vector2(0, 1),
        new Vector2(-1, 1)
    }.AsReadOnly();

    public static ReadOnlyCollection<Vector2> GetSegments(BlockProperties.BlockType blockType)
    {
        switch (blockType)
        {
            case BlockProperties.BlockType.None:
                break;
            case BlockProperties.BlockType.I:
                return ISegments;
            case BlockProperties.BlockType.J:
                return JSegments;
            case BlockProperties.BlockType.L:
                return LSegments;
            case BlockProperties.BlockType.O:
                return OSegments;
            case BlockProperties.BlockType.S:
                return SSegments;
            case BlockProperties.BlockType.T:
                return TSegments;
            case BlockProperties.BlockType.Z:
                return ZSegments;
        }

        return new ReadOnlyCollection<Vector2>(new List<Vector2>());
    }
}