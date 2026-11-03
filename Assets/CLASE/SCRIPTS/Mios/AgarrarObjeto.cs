using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class AgarrarObjeto : NetworkBehaviour
{
    [Header("Hand")]
    [SerializeField] private GameObject handPoitn;

    [Header("Interaction")]
    [SerializeField] private float pickupDistance = 3f;

    [Networked] private NetworkId PickedObjectId { get; set; }

    public Transform GetHandPoint()
    {
        return handPoitn != null ? handPoitn.transform : null;
    }

    private void Update()
    {
        if (!HasInputAuthority || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame && PickedObjectId == default)
            TryPickup();

        if (Keyboard.current.rKey.wasPressedThisFrame && PickedObjectId != default)
            RPC_RequestDrop();
    }

    private void TryPickup()
    {
        if (handPoitn == null)
            return;

        Collider[] colliders = Physics.OverlapSphere(handPoitn.transform.position, pickupDistance);
        NetworkObject closestObject = null;
        float closestDistance = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (!col.CompareTag("Objeto"))
                continue;

            NetworkObject netObj = col.GetComponentInParent<NetworkObject>();
            if (netObj == null)
                continue;

            Bomb bomb = netObj.GetComponent<Bomb>();
            if (bomb == null || bomb.IsBombPlanted())
                continue;

            float distance = Vector3.Distance(handPoitn.transform.position, netObj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestObject = netObj;
            }
        }

        if (closestObject != null)
            RPC_RequestPickup(closestObject.Id);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestPickup(NetworkId objectId)
    {
        if (PickedObjectId != default || handPoitn == null)
            return;

        if (!Runner.TryFindObject(objectId, out NetworkObject netObj))
            return;

        Bomb bomb = netObj.GetComponent<Bomb>();
        if (bomb == null || bomb.IsBombPlanted())
            return;

        if (Vector3.Distance(transform.position, netObj.transform.position) > pickupDistance + 2f)
            return;

        foreach (AgarrarObjeto player in FindObjectsByType<AgarrarObjeto>(FindObjectsSortMode.None))
        {
            if (player != this && player.PickedObjectId == objectId)
                return;
        }

        PickedObjectId = objectId;
        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority || PickedObjectId == default)
            return;

        if (!Runner.TryFindObject(PickedObjectId, out NetworkObject netObj))
        {
            PickedObjectId = default;
            return;
        }

        if (!GetInput(out NetworkInfoData input))
            return;

        // La posición se obtiene de la cámara/mano local del dueño,
        // pero solamente el host mueve el objeto de red.
        if (Vector3.Distance(transform.position, input.handPosition) > 4f)
            return;

        netObj.transform.SetPositionAndRotation(input.handPosition, input.handRotation);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestDrop()
    {
        if (PickedObjectId == default)
            return;

        if (!Runner.TryFindObject(PickedObjectId, out NetworkObject netObj))
        {
            PickedObjectId = default;
            return;
        }

        Rigidbody rb = netObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Bomb bomb = netObj.GetComponent<Bomb>();
        if (bomb != null)
            bomb.PlantBomb(Object.InputAuthority);

        PickedObjectId = default;
        Debug.Log("Bomba soltada por: " + Object.InputAuthority);
    }
}
