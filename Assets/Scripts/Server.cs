using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;


public class SocketServer : WebSocketBehavior
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
    private SocketServer socketServer;
    private WebSocketServer wss;
    
    void Awake()
    {
        wss = new WebSocketServer("ws://localhost:8888");
        wss.AddWebSocketService<SocketServer>("/Server");
        wss.Start();
    }

    void Update()
    {
        if(wss.IsListening)
        {
            Debug.Log("WSS is Running");
        }
    }
}