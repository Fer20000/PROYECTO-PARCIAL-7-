using Fusion;
using Fusion.Sockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using WebSocketSharp;

/// <summary>
/// Callback: Un metodo que se pasa a otra como argumento. Existe una funcion receptora que se ejecuta mas tarde. 
/// Su funcion principal es indicar que hacer justo despues de que concluya un proceso.
/// 
/// NetworkBehaviour lo que hace es exactamente lo mismo que el monobehaviour, pero, tomando en cuenta
/// que realizará funciones de red
/// </summary>
public class PhotonManager : MonoBehaviour, INetworkRunnerCallbacks // Interfaz de llamadas al servidor
{
    /// <summary>
    /// El Runner es el componente mas importante de photon, porque es el que manda a llamar la API. Es el que nos conecta con el servidor o host
    /// </summary>
    public static PhotonManager Instance;
    public NetworkRunner runner;

    [SerializeField] private UnityEvent onPlayerJoined;

    [SerializeField] private NetworkPrefabRef playerPrefab; // El prefab del player es un objeto importante. Lo que guarda es el ID

    [SerializeField] private Dictionary<PlayerRef, NetworkObject> players = new Dictionary<PlayerRef, NetworkObject>(); // Este diccionario me va a servir para tener todos los jugadores que hay en la partida guardados

    [SerializeField] private Transform spawnPoint;

    [SerializeField] private GameObject loadingCanvas;
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject startPanel;

    [SerializeField] private UnityEvent eventos;

    public static event Action<List<SessionInfo>> onSessionListUpdate;//Es un evento que guarda // Lobby Manager
    public static List<SessionInfo> sessionList = new List<SessionInfo>();

    private Handgun localHandgun;
    private bool pendingShot;

    private void Awake() // es lo primero que sucede al cargar la escena
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {
            Destroy(this.gameObject);
        }

            Debug.Log("Awake");
        runner = FindFirstObjectByType<NetworkRunner>();
    }

    private void Start() // Cuando inicia el script
    {
        Debug.Log("Start");
        runner.AddCallbacks(this); // Este runner va a tener vinculadas las callbacks de este script

        startPanel.SetActive(true);
        loadingCanvas.SetActive(false);
        mainMenuCanvas.SetActive(false);
    }


    private void Update()
    {
        if (startPanel.activeSelf && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            StartConnection();
        }
        if (runner != null && runner.IsRunning &&
              InputManager.Instance != null &&
              InputManager.Instance.IsMainFirePressed())
        {
            pendingShot = true;
        }
    }

    private async void StartConnection()
    {
        startPanel.SetActive(false);
        loadingCanvas.SetActive(true);

        await ConnectToServerLobby();

        loadingCanvas.SetActive(false);
        mainMenuCanvas.SetActive(true);
    }
    /// <summary>
    /// En este Lobby el server (photon) sabe que existes, de ahi tu puedes ir a cualquier lado
    /// ya sea una partida, una party o cualquier lugar del menú que el juego te permita
    /// 
    /// Desde el lobby tenemos acceso a saber todas las partidas que existen, que otros jugadores hay conectados
    /// </summary>
    /// <returns></returns>

    private async Task<bool> ConnectToServerLobby()
    {
        var result = await runner.JoinSessionLobby(
            SessionLobby.Custom,
            "Looby de pruebas para clase"
        );

        Debug.Log("Resultado de conexión al lobby: " + result.Ok);

        if (!result.Ok)
        {
            Debug.LogError("No se pudo conectar al lobby: " + result.ShutdownReason);
        }

        return result.Ok;
    }

    //public void OnSessionListUpdate(NetworkRunner runner, List<SessionInfo> sessionList)
    //{
    //    PhotonManager.sessionList = sessionList;
    //    onSessionListUpdate.Invoke(sessionList);
    //}
    //private async Task OnConnectToServer(NetworkRunner runner)
    //{

    //}

    // Tarea asincrona que nos inicia la partida

    [SerializeField] private TMPro.TMP_InputField sessionName;
    [SerializeField] private TMPro.TMP_InputField maxPlayers;
    private async Task StartCustomMatch(GameMode mode)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        Debug.Log("Session Name asignado: " + (sessionName != null));
        sessionName.text = sessionName.text.IsNullOrEmpty() ? GenericFunctions.RandomName(5) : sessionName.text;

        // argumentos son como los parametros de un metodo, sin embargo, argumentos se refiere a los datos que pide una llamada a la api
        await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = sessionName.text,
            Scene = sceneInfo,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = Mathf.Clamp(
               int.TryParse(maxPlayers.text, out int cantidad) ? cantidad : 2,
               2,
               10
            ),
        });
    }


    /// <summary>
    /// Esta tarea va dedicada a entrar a una sesion desde el boton Join Game de una sesion existente
    /// </summary>
    public async Task StartCustomMatch(GameMode mode, string serverName)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        var result = await runner.StartGame(new StartGameArgs
        {
            GameMode = mode,
            SessionName = serverName,
            Scene = sceneInfo,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),

            PlayerCount = 2
        });

        if (!result.Ok)
        {
            Debug.LogWarning("No se pudo entrar a la partida: " + result.ShutdownReason);

            loadingCanvas.SetActive(false);
        }
    }


    private async Task StartQuickMatch(GameMode mode)
    {
        runner.ProvideInput = true;

        var scene = SceneRef.FromIndex(0);

        var sceneInfo = new NetworkSceneInfo();
        sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        //sessionName.text = sessionName.text.IsNullOrEmpty() ? GenericFunctions.RandomName(5) : sessionName.text;

        await runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = GenericFunctions.RandomName(5), // esto me sirve para que solo haya una partida en todo el juego, no puede haber más de una sesion con el mismo nombre
            Scene = sceneInfo,
            SceneManager = runner.GetComponent<NetworkSceneManagerDefault>(),
            PlayerCount = 2
        });
    }

    public async void CreateCustomGame()
    {
        await StartCustomMatch(GameMode.Host);
    }
    public void CreateGame()
    {
        StartQuickMatch(GameMode.Host);
    }

    [ContextMenu("Nombre Random")]
    public void Algo()
    {
        Debug.Log(GenericFunctions.RandomName(5));
    }


    public void StartGameAsHost()
    {
        Task startGame = StartQuickMatch(GameMode.Host);
    }

    public void StartGameAsClient()
    {
        Task startGame = StartCustomMatch(GameMode.Client);
    }



    public async Task<bool> DisconnectFromGame()
    {
        NetworkRunner oldRunner = runner;

        if (oldRunner != null)
        {
            oldRunner.RemoveCallbacks(this);

            Debug.Log("[RECONEXION] Antes de Shutdown");

            if (!oldRunner.IsShutdown)
            {
                await oldRunner.Shutdown(destroyGameObject: false);
            }

            Debug.Log("[RECONEXION] Shutdown terminado");

            GameObject runnerObjectToDestroy = oldRunner.gameObject;

            Destroy(runnerObjectToDestroy);
            runner = null;

            await Task.Yield();

            while (runnerObjectToDestroy != null)
            {
                await Task.Delay(20);
            }
        }

        // Limpiar referencias de la partida anterior.
        players.Clear();
        localHandgun = null;
        pendingShot = false;
        sessionList.Clear();

        // Crear un NetworkRunner nuevo.

        GameObject runnerObject = new GameObject("FusionRunner");

        runner = runnerObject.AddComponent<NetworkRunner>();
        runnerObject.AddComponent<NetworkSceneManagerDefault>();

        runner.ProvideInput = true;
        runner.AddCallbacks(this);


        // Reconectar al lobby.
        bool connected = await ConnectToServerLobby();

        Debug.Log("[RECONEXION] Resultado: " + connected);

        return connected;
    }




    private bool isReturningToMenu = false;


    public async void ReturnToMenu()
    {
        if (isReturningToMenu)
            return;

        isReturningToMenu = true;

        startPanel.SetActive(false);
        mainMenuCanvas.SetActive(false);
        loadingCanvas.SetActive(true);

        try
        {
            bool connected = await DisconnectFromGame();

            if (connected)
            {
                mainMenuCanvas.SetActive(true);
                Debug.Log("Regreso al menú correctamente.");
            }
            else
            {
                startPanel.SetActive(true);
                Debug.LogError("No se pudo reconectar al lobby.");
            }
        }
        catch (Exception exception)
        {
            startPanel.SetActive(true);
            Debug.LogException(exception);
        }
        finally
        {
            loadingCanvas.SetActive(false);
            isReturningToMenu = false;
        }
    }

    private void OnDestroy()
    {
        Debug.Log("[RECONEXION] PhotonManager destruido.");
    }



    #region Metodos de Photon

    // Cuando un jugador entra a la partida
    // PlayerRef es un id que nos dice a cual jugador se refiere
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            Vector3 spawnPosition = new Vector3(UnityEngine.Random.Range(-3f, 3f), 2f, UnityEngine.Random.Range(-3f, 3f));
            players.Add(player, runner.Spawn(playerPrefab, spawnPosition, Quaternion.identity, player));
        }
        onPlayerJoined?.Invoke();
    }

    // Cuando un jugador se sale
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if(players.ContainsKey(player))
        {
            runner.Despawn(players[player]);
            players.Remove(player);
        }
    }


    /// <summary>
    /// Photon debe de evaluar todos los inputs que realiza el usuario
    /// 
    /// Estos inputs photon los recibe como un struct, esto nosotros lo podemos saber 
    /// gracias a que el callback que recibe OnInput, recibe de parametro un struct de tipo
    /// NetworkInput
    /// <summary>


    // Cunado hay inputs de red, y se detecta alguno de ellos
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        if (InputManager.Instance == null)
            return;

        if (localHandgun == null)
        {
            Handgun[] handguns =
                FindObjectsByType<Handgun>(FindObjectsSortMode.None);

            foreach (Handgun handgun in handguns)
            {
                if (handgun.Object != null &&
                    handgun.HasInputAuthority)
                {
                    localHandgun = handgun;
                    break;
                }
            }
        }

        NetworkInfoData data = new NetworkInfoData()
        {
            movementAxis = InputManager.Instance.GetMoveInput(),
            runInputPressed = InputManager.Instance.WasRunInputPressed(),
            movingBackwards = InputManager.Instance.IsMovingBackwards(),
            movingOnXAxis = InputManager.Instance.IsMovingOnXAxis(),
            moveInputPressed = InputManager.Instance.IsMoveInputPressed(),
            mousePos = InputManager.Instance.GetMouseDelta(),
            isShooting = pendingShot
        };

        if (localHandgun != null &&
            localHandgun.shootPoint != null)
        {
            data.shootPosition = localHandgun.shootPoint.position;
            data.shootDirection = localHandgun.shootPoint.forward;
        }
        AgarrarObjeto[] grabbers =
    FindObjectsByType<AgarrarObjeto>(FindObjectsSortMode.None);

        foreach (AgarrarObjeto grabber in grabbers)
        {
            if (grabber.Object == null || !grabber.HasInputAuthority)
                continue;

            Transform hand = grabber.GetHandPoint();

            if (hand != null)
            {
                data.handPosition = hand.position;
                data.handRotation = hand.rotation;
            }

            break;
        }
        input.Set(data);
        pendingShot = false;
    }




    // Este metodo se manda a llamar cuando te conectas al servidor, ya sea como cliente o como host
    public void OnConnectedToServer(NetworkRunner runner)
    {
        mainMenuCanvas.SetActive(false);
    }

    // Cuando te intentas conectar pero no lo logra
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log(reason);
    }

    // Cuando empieza a intentar conectarte al servidor
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    // Cuando te desconectas, cuando se te va el internet o cuando cierras el juego
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {

    }

    // Cuando alguien se sale de la partida y tienes activada la migracion de host
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    // Cuando hay inputs de red, y se detecta alguno de ellos
    //public void OnInput(NetworkRunner runner, NetworkInput input)
    //{

    //}

    // Cuando se intenta mandar un input pero se pierde paquete
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    //// Cuando un jugador se sale
    //public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    //{

    //}

    // Cuando carga una escena
    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    // Cuando empieza a cargar una escena
    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    // Cuando se crea una nueva partida y cuando se termina o elimina una partida, funciona tanto para publicas como privadas
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        PhotonManager.sessionList = sessionList;
        onSessionListUpdate.Invoke(sessionList);

        Debug.Log("Sesiones encontradas: " + sessionList.Count);

        foreach (SessionInfo session in sessionList)
        {
            Debug.Log("Servidor disponible: " + session.Name);
        }

    }

    // Cuando el host o el server le damos en apagar
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log(
            "[RECONEXION] OnShutdown recibido. Motivo: "
            + shutdownReason
        );
    }

    #endregion



    #region Basura
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {

    }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {

    }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {

    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    } 
    #endregion
}
