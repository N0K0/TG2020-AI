using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class ClientController : WebSocketBehavior
{
    public GameObject carController = null;
    private Server server = null;

    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
        parseCommand(e.ToString());
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
        if(carController == null)
        {
            return;
        }

        CarController cc = carController.GetComponent<CarController>();
        if(cc != null)
        {
            cc.KickPlayer();
        }
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Exception);
    }

    public void Close(string reason) {
        base.Close(1001, reason);
    }

    public void parseCommand(string command)
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
    public Server server = null;
    public string UserName = "Anon";

    public bool Playable = true; // Used to check is a player has been DQed. Can't remove the object before after a round is done due to score
    public bool Active = true;

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TickUpdate() { 
    }

    public void KickPlayer()
    {
        // This function simply calls the server (which job is to deal with leaving players)
        server.KickPlayer(this);
        // Let the server figure out how to kick the players
    }

    public void StopPlayer()
    {
        Playable = false;
        Active = false;
        clientController.Close("CarController stopped");
    }

    public void OnDestroy()
    {
        clientController.Close("CarController was destroyed");
    }

}
