using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : NetworkBehaviour
{
    public enum WinReason
    {
        None,
        PlayerKilled,
        BombExploded,
        BombDefused
    }


    [Networked]
    private NetworkBool GameFinished { get; set; }

    [Networked]
    private PlayerRef Winner { get; set; }

    [Networked]
    private WinReason Reason { get; set; }


    [Header("UI")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;


    public override void Spawned()
    {
        if (HasStateAuthority)
        {
            GameFinished = false;
            Winner = PlayerRef.None;
            Reason = WinReason.None;
        }

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }


    public void PlayerKilled(PlayerRef winner)
    {
        if (!HasStateAuthority)
            return;

        if (GameFinished)
            return;

        EndGame(
            winner,
            WinReason.PlayerKilled
        );
    }


    public void PlayerExplodedBomb(PlayerRef bombOwner)
    {
        if (!HasStateAuthority)
            return;

        if (GameFinished)
            return;

        EndGame(
            bombOwner,
            WinReason.BombExploded
        );
    }


    public void PlayerDefusedBomb(PlayerRef player)
    {
        if (!HasStateAuthority)
            return;

        if (GameFinished)
            return;

        EndGame(
            player,
            WinReason.BombDefused
        );
    }


    private void EndGame(
        PlayerRef winner,
        WinReason winReason)
    {
        GameFinished = true;
        Winner = winner;
        Reason = winReason;

        RPC_ShowResult(winner);
    }



    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_ShowResult(PlayerRef winner)
    {
        // Liberar el mouse al terminar la partida
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        bool playerWon = Runner.LocalPlayer == winner;

        if (playerWon)
        {
            if (winPanel != null)
                winPanel.SetActive(true);

            if (losePanel != null)
                losePanel.SetActive(false);

            Debug.Log("GANASTE");
        }
        else
        {
            if (losePanel != null)
                losePanel.SetActive(true);

            if (winPanel != null)
                winPanel.SetActive(false);

            Debug.Log("PERDISTE");
        }
    }

}
