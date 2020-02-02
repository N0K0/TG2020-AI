using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;

public class MultiplayerHandler : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
    }

    protected override void OnOpen()
    {
        Debug.Log("New connection open!");
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log(e.Reason);
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Message);
    }

}

public class Server : MonoBehaviour
{
    private MultiplayerHandler socketServer;
    private WebSocketServer wss;
    private WebSocketServiceHost serviceHost;
        
    void Awake()
    {
        wss = new WebSocketServer("ws://localhost:8888");
        wss.AddWebSocketService<MultiplayerHandler>("/server");
        wss.Start();
        serviceHost = wss.WebSocketServices["/server"];

    }

    void Update()
    {
        print(serviceHost);
    }
}