using Fusion;
using UnityEngine;

public class Handgun : Weapon
{
    public override void PhysicShoot()
    {
        if (!HasStateAuthority)
            return;

        if (shootPoint == null)
            return;

        SpawnBullet(
            shootPoint.position,
            shootPoint.rotation,
            Object.InputAuthority
        );
    }

    public void SpawnBullet(
        Vector3 position,
        Quaternion rotation,
        PlayerRef shooter)
    {
        if (!HasStateAuthority)
            return;

        Runner.Spawn(
            projectible,
            position,
            rotation,
            shooter,
            (runner, obj) =>
            {
                BulletKCC bullet = obj.GetComponent<BulletKCC>();

                if (bullet != null)
                {
                    bullet.Initialize(
                        rotation * Vector3.forward,
                        shooter
                    );
                }
            }
        );
    }

    public override void RaycastShoot()
    {
        if (!HasStateAuthority)
            return;

        if (shootPoint == null)
            return;

        if (Physics.Raycast(
            shootPoint.position,
            shootPoint.forward,
            out RaycastHit hit,
            maxDistance,
            layers))
        {
            Debug.Log("Raycast golpeó: " + hit.collider.name);
        }
    }

    public override void Reload()
    {
    }
}