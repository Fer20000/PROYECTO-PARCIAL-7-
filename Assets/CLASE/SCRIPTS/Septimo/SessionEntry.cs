using TMPro;
using UnityEngine;

/// <summary>
/// Este script se encuentra en el prefab de Session (El prefab del canvas donde se muestran los datos de una session)
/// </summary>

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TMP_Text serverName;
    [SerializeField] private TMP_Text mapName;
    [SerializeField] private TMP_Text gameMode;
    [SerializeField] private TMP_Text currentPlayers;

    public void SetInfo(ServerDataStruct serverData)
    {
        serverName.text = serverData.serverName;
        currentPlayers.text = serverData.currentPlayers + " / " + serverData.maxPlayers;

        mapName.text = serverData.mapName;
        gameMode.text = serverData.gameMode;

    }

    public void JoinGame()
    {
        PhotonManager.Instance.StartCustomMatch(Fusion.GameMode.Client, serverName.text);
    }
}

