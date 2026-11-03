using Fusion;
using UnityEngine;
using UnityEngine.InputSystem;

public class Bomb : NetworkBehaviour
{
    [Header("Bomb")]
    [SerializeField] private float explosionTime = 60f;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private int explosionDamage = 100;
    private const float MaxDefuseDistance = 3.25f;

    [Header("Effect")]
    [SerializeField] private GameObject explosionEffect;


    [Networked]
    private NetworkBool IsPlanted { get; set; }

    [Networked]
    private NetworkBool GameFinished { get; set; }

    [Networked]
    private PlayerRef BombOwner { get; set; }

    [Networked]
    private TickTimer ExplosionTimer { get; set; }


    public bool IsBombPlanted()
    {
        return IsPlanted;
    }


    public PlayerRef GetBombOwner()
    {
        return BombOwner;
    }


    // ==========================================
    // PLANTAR
    // ==========================================

    public void PlantBomb(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (IsPlanted)
            return;

        if (GameFinished)
            return;


        IsPlanted = true;

        // Guardamos qui�n la plant�
        BombOwner = player;


        // Comienza el contador de 10 segundos
        ExplosionTimer =
            TickTimer.CreateFromSeconds(
                Runner,
                explosionTime
            );


        Debug.Log(
            "BOMBA PLANTADA POR: " +
            BombOwner
        );
    }


    // ==========================================
    // TEMPORIZADOR
    // ==========================================

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority)
            return;

        if (!IsPlanted)
            return;

        if (GameFinished)
            return;


        if (ExplosionTimer.Expired(Runner))
        {
            Explode();
        }
    }


    // ==========================================
    // EXPLOTAR
    // ==========================================

    private void Explode()
    {
        if (!HasStateAuthority)
            return;

        if (GameFinished)
            return;


        Debug.Log(
            "���BOMBA EXPLOT�!!!"
        );


        // Buscar jugadores dentro del radio
        Collider[] players =
            Physics.OverlapSphere(
                transform.position,
                explosionRadius
            );


        foreach (Collider collider in players)
        {
            Health health =
                collider.GetComponentInParent<Health>();


            if (health == null)
                continue;


            NetworkObject playerObject =
                health.GetComponent<NetworkObject>();


            if (playerObject == null)
                continue;


            PlayerRef player =
                playerObject.InputAuthority;


            // IMPORTANTE:
            // El jugador que plant� NO recibe da�o
            if (player == BombOwner)
                continue;


            // El jugador contrario recibe da�o
            health.TakeDamage(
                explosionDamage,
                BombOwner
            );

        }

        // Mostrar explosi�n

        RPC_ShowExplosion(transform.position);

        // AQU� se llama al RPC
        RPC_ShowWinner(BombOwner);


        IsPlanted = false;

        GameFinished = true;

        ExplosionTimer = TickTimer.None;
    }


    // ==========================================
    // DESACTIVAR
    // ==========================================

    public void DefuseBomb(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (!IsPlanted)
            return;

        if (GameFinished)
            return;


        // El que plant� no puede desactivar
        if (player == BombOwner)
            return;


        Debug.Log(
            "BOMBA DESACTIVADA POR: " +
            player
        );


        IsPlanted = false;

        GameFinished = true;

        ExplosionTimer = TickTimer.None;


        // El jugador que desactiv� gana
        RPC_ShowWinner(
            player
        );
    }


    // ==========================================
    // EXPLOSI�N VISUAL
    // ==========================================

    [Rpc(
        RpcSources.StateAuthority,
        RpcTargets.All
    )]
    private void RPC_ShowExplosion(
        Vector3 position)
    {
        if (explosionEffect != null)
        {
            Instantiate(
                explosionEffect,
                position,
                Quaternion.identity
            );
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            explosionRadius
        );
    }

    [Rpc(
    RpcSources.All,
    RpcTargets.StateAuthority
)]
    public void RPC_PlantBomb(
    PlayerRef player,
    RpcInfo info = default)
    {
        PlantBomb(player);
    }


    private void OnTriggerStay(Collider other)
    {
        if (!IsPlanted)
            return;


        NetworkObject playerObject =
            other.GetComponentInParent<NetworkObject>();


        if (playerObject == null)
            return;


        if (!playerObject.HasInputAuthority)
            return;


        if (playerObject.InputAuthority == BombOwner)
            return;


        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            RPC_RequestDefuse();
        }
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    private void RPC_RequestDefuse(RpcInfo info = default)
    {
        if (!HasStateAuthority || !IsPlanted || GameFinished)
            return;

        PlayerRef player = info.Source;
        if (player == PlayerRef.None || player == BombOwner)
            return;

        NetworkObject requestingPlayer = null;
        AgarrarObjeto[] grabbers = FindObjectsByType<AgarrarObjeto>(FindObjectsSortMode.None);
        foreach (AgarrarObjeto grabber in grabbers)
        {
            if (grabber == null || grabber.Object == null || grabber.Runner != Runner)
                continue;

            if (grabber.Object.InputAuthority != player)
                continue;

            requestingPlayer = grabber.Object;
            break;
        }

        if (requestingPlayer == null)
            return;

        if (Vector3.Distance(requestingPlayer.transform.position, transform.position) > MaxDefuseDistance)
            return;

        DefuseBomb(player);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowWinner(PlayerRef winner)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        GameUI gameUI = FindFirstObjectByType<GameUI>();

        if (gameUI == null)
            return;

        if (Runner.LocalPlayer == winner)
        {
            gameUI.MostrarGanaste();
        }
        else
        {
            gameUI.MostrarPerdiste();
        }
    }

}