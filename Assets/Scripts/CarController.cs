using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class ClientController : WebSocketBehavior
{

    private CarController carController = null;

    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
        carController.OnMessage(e);
    }

    protected override void OnOpen()
    {
        Debug.Log("New connection open!");
        Debug.Log(this.Sessions.Count);
        carController.OnOpen();
    }

    protected override void OnClose(CloseEventArgs e)
    {
        Debug.Log(e.Reason);
        carController.OnClose(e);
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Message);
        carController.OnError(e);
    }
}

public class CarController : MonoBehaviour
{

    private ClientController clientController = null;


    public void ParseCommand()
    {

    }

    public void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
    }

    public void OnOpen()
    {
        Debug.Log("New connection open!");
        Debug.Log(clientController);
    }

    public void OnClose(CloseEventArgs e)
    {
        Debug.Log(e.Reason);
    }

    public void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Message);
    }

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
