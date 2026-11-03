using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private GameObject noSessionsFoundMsg;
    [SerializeField] private GameObject sessionPrefab;
    [SerializeField] private Transform viewportContent;

    private Dictionary<string, SessionEntry> sessions = new Dictionary<string, SessionEntry>();

    private void OnEnable()
    {
        PhotonManager.onSessionListUpdate += OnSessionListUpdate;
    }

    private void OnDisable()
    {
        PhotonManager.onSessionListUpdate -= OnSessionListUpdate;
    }

    public void OnSessionListUpdate(List<SessionInfo> sessionList)
    {
        noSessionsFoundMsg.SetActive(sessionList.Count == 0);

        List<string> sessionsToRemove = new List<string>();

        foreach (string sessionName in sessions.Keys)
        {
            bool found = false;

            foreach (SessionInfo sessionInfo in sessionList)
            {
                if (sessionInfo.Name == sessionName)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                sessionsToRemove.Add(sessionName);
            }
        }

        foreach (string sessionName in sessionsToRemove)
        {
            if (sessions[sessionName] != null)
            {
                Destroy(sessions[sessionName].gameObject);
            }

            sessions.Remove(sessionName);
        }

        for (int i = 0; i < sessionList.Count; i++)
        {
            ServerDataStruct serverData = new ServerDataStruct();

            serverData.serverName = sessionList[i].Name;
            serverData.currentPlayers = sessionList[i].PlayerCount;
            serverData.maxPlayers = sessionList[i].MaxPlayers;

            if (sessions.ContainsKey(serverData.serverName))
            {
                sessions[serverData.serverName].SetInfo(serverData);
            }
            else
            {
                GameObject session = Instantiate(sessionPrefab, viewportContent);

                SessionEntry entry = session.GetComponent<SessionEntry>();

                entry.SetInfo(serverData);

                sessions.Add(serverData.serverName, entry);
            }
        }
    }
}