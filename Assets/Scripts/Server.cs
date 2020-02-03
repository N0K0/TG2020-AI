using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;

public class Server : MonoBehaviour
{
    private ClientController socketServer;
    private WebSocketServer wss;
    private WebSocketServiceHost serviceHost;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        wss = new WebSocketServer("ws://localhost:8888");
        wss.AddWebSocketService<ClientController>("/server");
        wss.Start();
    }

    public void StartServer()
    {
        serviceHost = wss.WebSocketServices["/server"];
        WebSocketSessionManager clients = serviceHost.Sessions;
    }

    void Update()
    {
    }
}