using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;


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

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        wss = new GameSocketServer("ws://localhost:8888");
        wss.AddWebSocketService<ClientController>("/server", () => new ClientController(this) { } ) ;
        wss.Start();
    }

    public void GetSessions()
    {

    }

    public void RegisterClient(ClientController client)
    {
        CarController player = new CarController();
        client.carController = player;
        player.clientController = client;
        players.Add(player);
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
        // Update sessions

        // Update car controllers
    }
}