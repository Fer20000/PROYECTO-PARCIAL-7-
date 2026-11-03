
using UnityEngine;
using Fusion;
using System.Threading.Tasks;

public class Projectible : NetworkBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float lifeTime;

    private Rigidbody rb;

    public override void Spawned()
    {
        rb = GetComponent<Rigidbody>();

        if (!HasStateAuthority)
            return;

        rb.linearVelocity = transform.forward * speed;

        DespawnAfterTime();
    }

    private async void DespawnAfterTime()
    {
        await Task.Delay((int)(lifeTime * 1000));

        if (Runner != null && Runner.IsRunning && Object != null && Object.IsValid && HasStateAuthority)
            Runner.Despawn(Object);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Object != null && Object.IsValid && HasStateAuthority)
            Runner.Despawn(Object);
    }
}
