using System.Collections.Generic;
using UnityEngine;

public class DeadBlocksEffect : MonoBehaviour
{
    [SerializeField] private BoardData _boardData;
    [SerializeField] private List<ParticleSystem> _particleSystems;

    private void Start()
    {
        _boardData.OnBoardClearEvent += StopParticles;
        _boardData.OnLevelChanged += BoostParticles;
    }

    private void BoostParticles()
    {
        int indexToAdd = Random.Range(0, _particleSystems.Count);
        ParticleSystem.EmissionModule emission = _particleSystems[indexToAdd].emission;
        emission.rateOverTimeMultiplier += 0.2f;
    }

    private void StopParticles()
    {
        foreach (ParticleSystem system in _particleSystems)
        {
            ParticleSystem.EmissionModule emission = system.emission;
            emission.rateOverTimeMultiplier = 0;
        }
    }
}