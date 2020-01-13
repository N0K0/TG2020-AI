using UnityEngine;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;

public class Server : MonoBehaviour
{

    void Awake()
    {
        print("test");
        WebSocketServer wss = new WebSocketServer("ws://localhost:8888");
        wss.AddWebSocketService<>
    }
}