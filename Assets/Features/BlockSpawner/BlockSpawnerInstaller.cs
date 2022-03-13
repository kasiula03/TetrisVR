using UnityEngine;
using Zenject;

public class BlockSpawnerInstaller : MonoInstaller
{
    [SerializeField] private Block _iBlock;
    [SerializeField] private Block _jBlock;
    [SerializeField] private Block _lBlock;
    [SerializeField] private Block _oBlock;
    [SerializeField] private Block _sBlock;
    [SerializeField] private Block _tBlock;
    [SerializeField] private Block _zBlock;
    [SerializeField] private TimersManager _timersManager;

    public override void InstallBindings()
    {
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("I").FromComponentInNewPrefab(_iBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("J").FromComponentInNewPrefab(_jBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("L").FromComponentInNewPrefab(_lBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("O").FromComponentInNewPrefab(_oBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("S").FromComponentInNewPrefab(_sBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("T").FromComponentInNewPrefab(_tBlock);
        Container.BindMemoryPool<Block, BlockPool.Pool>().WithId("Z").FromComponentInNewPrefab(_zBlock);
        Container.Bind<BlockPool>().AsSingle();
        Container.BindInstance(_timersManager);
    }
}