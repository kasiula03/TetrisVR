using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

public class BlockSpawner : MonoBehaviour
{
    [SerializeField] private Transform _parent;

    [Inject] private BlockPool _pool;

    private readonly List<BlockProperties>
        _queueToSpawn = new List<BlockProperties>(); //TODO: Because we want to display it before spawns
    
    private Action<Vector2> _onBlockMoved;
    private Action<Block> _moveToBottom;
    private Action<Block, Vector2> _moveBlock;
    private Action<Block, int> _rotateBlock;
    private int _blocksInQueue = 2;
    
    public void Setup(Action<Vector2> onBlockMoved, Action<Block> moveToBottom, Action<Block, Vector2> moveBlock, Action<Block, int> rotateBlock)
    {
        _onBlockMoved = onBlockMoved;
        _moveToBottom = moveToBottom;
        _moveBlock = moveBlock;
        _rotateBlock = rotateBlock;

        for (int i = 0; i < _blocksInQueue; i++)
        {
            BlockProperties.BlockType blockType = (BlockProperties.BlockType) Random.Range(1, 8);
            _queueToSpawn.Add(new BlockProperties(blockType));
        }
    }

    public Block SpawnBlock(Vector3 spawnPosition)
    {
        BlockProperties.BlockType blockType = (BlockProperties.BlockType) Random.Range(1, 8);
        _queueToSpawn.Add(new BlockProperties(blockType));
        BlockProperties properties = _queueToSpawn.First();
        Block block = _pool.Spawn(properties, _parent, spawnPosition, _onBlockMoved, _moveToBottom, _moveBlock, _rotateBlock);
        _queueToSpawn.Remove(properties);
        return block;
    }
}