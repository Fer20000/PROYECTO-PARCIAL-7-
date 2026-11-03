using UnityEngine;
using Fusion;

public class DestroyObjects : NetworkBehaviour
{
    [SerializeField] private float lifetime = 5f;
    private float _spawnTime;

    public override void Spawned()
    {
        _spawnTime = Runner.SimulationTime;
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer && Runner.SimulationTime - _spawnTime > lifetime)
        {
            Runner.Despawn(Object);
        }
    }

}
