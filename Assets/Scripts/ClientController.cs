using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;

public class ClientController : WebSocketBehavior
{
    public GameObject carController = null;
    internal Server server = null;
    internal CarController carComponent = null;

    protected override void OnMessage(MessageEventArgs e)
    {
        Debug.Log(e.ToString());
        parseMessage(e.ToString());
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
        if (carController == null)
        {
            return;
        }

        if (carComponent != null)
        {
            carComponent.KickPlayer();
        }
    }

    protected override void OnError(ErrorEventArgs e)
    {
        Debug.LogError(e.Exception);
    }

    public void Close(string reason)
    {
        base.Close(1001, reason);
    }

    public void parseMessage(string message)
    {
        BasicMessage msg = JsonConvert.DeserializeObject<BasicMessage>(message);

        switch (msg.Command)
        {
            // Misc commands (same as doc)
            case "Username":
                break;
            case "Color":
                break;

            // Useful commands


            default:
                Debug.LogError("User sent command i do not know what is");
                Debug.LogError(message);
                break;
        }
    }

    public void SetUsername(string username)
    {
        // The ClientController is not allowed to talk on behalf of the Player, must pass it via CarController
        carComponent.SetUsername(username);
    }


    public ClientController(Server server)
    {
        this.server = server;
    }

    internal void RequestUsername()
    {
        Debug.Log("Requesting username");
        BasicMessage message = new BasicMessage();
        message.RequestUsername();
        string msg = JsonConvert.SerializeObject(message);
        Debug.Log(msg);
        Send(msg);
    }
}
