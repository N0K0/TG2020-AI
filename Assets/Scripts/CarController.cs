using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class ClientController : WebSocketBehavior
{

    public CarController carController = null;
    private Server server = null;

    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
    }

    protected override void OnOpen()
    {
        Debug.Log("New connection open!");
        Debug.Log(this.Sessions.Count);
        server.RegisterClient(this);
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log(e.Reason);
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Message);
    }

    public void parseCommand()
    {

    }

    public ClientController(Server server)
    {
        this.server = server;
    }
}

public class CarController : MonoBehaviour
{

    public ClientController clientController = null;
    public string Name;

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
