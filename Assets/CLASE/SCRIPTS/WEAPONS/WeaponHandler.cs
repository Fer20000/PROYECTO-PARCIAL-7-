using Fusion;
using UnityEngine;

public class WeaponHandler : NetworkBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Weapon currentWeapon;

    private bool previousShooting;

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!GetInput(out NetworkInfoData input))
            return;

        bool shooting = input.isShooting;

        if (shooting && !previousShooting && currentWeapon != null)
        {
            if (currentWeapon.shootType == ShootType.Physic)
            {
                Handgun handgun = currentWeapon as Handgun;

                if (handgun != null &&
                    input.shootDirection.sqrMagnitude > 0.001f)
                {
                    handgun.SpawnBullet(
                        input.shootPosition,
                        Quaternion.LookRotation(input.shootDirection),
                        Object.InputAuthority
                    );
                }
            }
            else
            {
                currentWeapon.RaycastShoot();
            }
        }

        previousShooting = shooting;
    }
}