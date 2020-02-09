using Newtonsoft.Json;
using System;
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
        Debug.Log(e.Data);

        try
        {
            parseMessage(e.Data);
        } catch (Exception exce)
        {
            Debug.LogError(exce.Message);
        }

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
        Message msg = JsonConvert.DeserializeObject<Message>(message);

        switch (msg.Type)
        {
            // Misc commands (same as doc)
            case "Username":
                try
                {
                    SetUsername(msg.Command);

                } catch (InvalidCommandException e)
                {
                    // TODO: This outght to be an method
                    Message error = new Message();
                    error.Type = msg.Type;
                    error.Status = "Error";
                    error.Command = e.Message;
                    string str = JsonConvert.SerializeObject(error);
                    Send(str);
                }
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
        Message message = new Message();
        message.RequestUsername();
        string msg = JsonConvert.SerializeObject(message);
        Debug.Log(msg);
        Send(msg);
    }
}

[Serializable]
public class InvalidCommandException : Exception
{
    public InvalidCommandException(string error) : base(error) { }
}