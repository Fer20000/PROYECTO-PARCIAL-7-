using Fusion;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.UI.GridLayoutGroup;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private float fuerzaBala;
    [SerializeField] private float lifeTime;

    [SerializeField] private NetworkObject bulletHole;
    [SerializeField] private NetworkObject sparkles;

    public PlayerRef ownerRef { get; set; }

    public void OwnerMethod(PlayerRef owner)
    {
        ownerRef = owner;
    }

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(transform.forward * fuerzaBala, ForceMode.Impulse);

            DespawnAfterTime();
        }
    }

    private async void DespawnAfterTime()
    {
        await Task.Delay((int)(lifeTime * 1000));

        if (Object == null)
            return;

        if (Object.HasStateAuthority)
            Runner.Despawn(Object);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;

        if (collision.gameObject.CompareTag("Terreno"))
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 hitPoint = contact.point;
            Vector3 hitNormal = contact.normal;

            RpcSpawnBulletHole(hitPoint, hitNormal);


            if (Object.HasStateAuthority)
                Runner.Despawn(Object);
        }

    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSpawnBulletHole(Vector3 hitPoint, Vector3 hitNormal, RpcInfo info = default)
    {
        Runner.Spawn(bulletHole, hitPoint + (hitNormal * 0.1f), Quaternion.FromToRotation(Vector3.up, hitNormal));
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RpcSpawnSparkles(Vector3 hitPoint, RpcInfo info = default)
    {
        Runner.Spawn(sparkles, hitPoint, Quaternion.identity);
    }
}
