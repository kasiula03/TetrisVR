using System;
using UnityEngine;
using Zenject;

public class BlockPool
{
    [Inject(Id = "I")] private Pool _iPool;
    [Inject(Id = "J")] private Pool _jPool;
    [Inject(Id = "L")] private Pool _lPool;
    [Inject(Id = "O")] private Pool _oPool;
    [Inject(Id = "S")] private Pool _sPool;
    [Inject(Id = "T")] private Pool _tPool;
    [Inject(Id = "Z")] private Pool _zPool;

    public class Pool : MonoMemoryPool<Block>
    {
        public Block Spawn(BlockProperties properties, Transform parent, Vector3 position)
        {
            Block obj = Spawn();
            obj.transform.SetParent(parent, true);
            obj.transform.position = position;
            obj.transform.localScale = Vector3.one / 5;
            return obj;
        }
    }

    
    public Block Spawn(BlockProperties properties, Transform parent, Vector3 position, Action<Vector2> onBlockMoved,  Action<Block> moveToBottom, Action<Block, Vector2> moveBlock, Action<Block, int> rotateBlock)
    {
        Pool pool = GetPool(properties.Type);

        Block obj = pool.Spawn(properties, parent, position);
        obj.Initialize(properties, onBlockMoved,  moveToBottom, moveBlock, rotateBlock);
        return obj;
    }

    private Pool GetPool(BlockProperties.BlockType blockType)
    {
        switch (blockType)
        {
            case BlockProperties.BlockType.I:
                return _iPool;
            case BlockProperties.BlockType.J:
                return _jPool;
            case BlockProperties.BlockType.L:
                return _lPool;
            case BlockProperties.BlockType.O:
                return _oPool;
            case BlockProperties.BlockType.S:
                return _sPool;
            case BlockProperties.BlockType.T:
                return _tPool;
            case BlockProperties.BlockType.Z:
                return _zPool;
            default:
                throw new ArgumentOutOfRangeException(nameof(blockType), blockType, null);
        }
    }
}