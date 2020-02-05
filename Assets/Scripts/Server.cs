using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class GameSocketServer : WebSocketServer
{
    public GameSocketServer(string url) : base(url)
    {

    }
}

public class Server : MonoBehaviour
{
    private WebSocketServer wss = null;
    private WebSocketServiceHost serviceHost = null;

    private List<CarController> players = null;
    private GameObject car_prefab = null;

    private ConcurrentQueue<ClientController> clientsPending = new ConcurrentQueue<ClientController>();

    private void Start()
    {
        car_prefab = Resources.Load<GameObject>("CarController");
    }

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        players = new List<CarController>();
        wss = new GameSocketServer("ws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
        wss.AddWebSocketService<ClientController>("/server", () => new ClientController(this) { } ) ; 
        wss.Start();
    }

    public void RegisterClient(ClientController client)
    {
        clientsPending.Enqueue(client);
    }

    public void CreateCarControllerFromClient(ClientController client)
    {
        GameObject car_gameobject = Instantiate<GameObject>(car_prefab);
        client.carController = car_gameobject;
        CarController carcontroller_component = car_gameobject.GetComponent<CarController>();
        players.Add(carcontroller_component);
    }

    public List<CarController> GetPlayers()
    {
        return players;
    }

    public void StartServer()
    {
        serviceHost = wss.WebSocketServices["/server"];
        WebSocketSessionManager clients = serviceHost.Sessions;
    }

    void Update()
    {
        // Update car controllers
        ClientController client;
        if(clientsPending.TryDequeue(out client))
        {
            CreateCarControllerFromClient(client);
        }
        
    }
}