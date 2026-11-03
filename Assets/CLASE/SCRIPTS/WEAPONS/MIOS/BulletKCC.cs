
using Fusion;
using UnityEngine;

public class BulletKCC : NetworkBehaviour
{
    [SerializeField] private float speed = 40f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float radius = 0.1f;
    [SerializeField] private int damage = 12;

    [SerializeField] private NetworkObject bulletHole;
    [SerializeField] private NetworkObject sparkles;

    private Vector3 moveDirection;
    private PlayerRef ownerRef;

    [Networked] private TickTimer lifeTimer { get; set; }

    public void Initialize(Vector3 direction, PlayerRef owner)
    {
        moveDirection = direction.normalized;
        ownerRef = owner;
    }

    public override void Spawned()
    {
        if (!HasStateAuthority)
            return;

        if (moveDirection.sqrMagnitude < 0.001f)
            moveDirection = transform.forward;

        lifeTimer = TickTimer.CreateFromSeconds(Runner, lifeTime);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (lifeTimer.Expired(Runner))
        {
            Runner.Despawn(Object);
            return;
        }

        float distance = speed * Runner.DeltaTime;

        if (Physics.SphereCast(
            transform.position,
            radius,
            moveDirection,
            out RaycastHit hit,
            distance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            Health playerHealth =
                hit.collider.GetComponentInParent<Health>();

            if (playerHealth != null)
            {
                // No dañar al jugador que disparó
                if (playerHealth.Object.InputAuthority == ownerRef)
                {
                    transform.position += moveDirection * distance;
                    return;
                }

                Debug.Log(
                    "La bala golpeó a: " + playerHealth.name
                );

                playerHealth.TakeDamage(damage, ownerRef);

                Runner.Despawn(Object);
                return;
            }

            // Impacto contra el terreno
            if (hit.collider.CompareTag("Terreno"))
            {
                if (sparkles != null)
                {
                    Runner.Spawn(
                        sparkles,
                        hit.point,
                        Quaternion.identity
                    );
                }

                if (bulletHole != null)
                {
                    Runner.Spawn(
                        bulletHole,
                        hit.point + hit.normal * 0.1f,
                        Quaternion.FromToRotation(
                            Vector3.up,
                            hit.normal
                        )
                    );
                }
            }

            Runner.Despawn(Object);
            return;
        }

        transform.position += moveDirection * distance;
    }
}
