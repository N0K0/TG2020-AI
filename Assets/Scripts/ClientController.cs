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


    internal int mapRequestsLeft = 5;

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

    void username(Message msg)
    {
        try
        {
            SetUsername(msg.Command);
        }
        catch (InvalidCommandException e)
        {
            // TODO: This outght to be an method
            Message error = new Message();
            error.Error(msg.Type, e.Message);
            Send(JsonConvert.SerializeObject(error));
        }
    }

    void color(Message msg)
    {

    }

    void fullmap(Message msg)
    {
        try
        {
            Message data = new Message();
            data.MapStatus(server.mapStatusStr);

            if (mapRequestsLeft < 1)
            {
                throw new InvalidCommandException("No more map requests left");
            }

        }
        catch (InvalidCommandException e)
        {
            Message error = new Message();
            error.Error(msg.Type, e.Message);
            Send(JsonConvert.SerializeObject(error));
        }
    }

    void allplayers(Message msg)
    {
        throw new NotImplementedException();
    }

    void moveToPoints(Message msg)
    {
        carComponent.setLockon(true);
        throw new NotImplementedException();
    }

    void turnAngle(Message msg)
    {
        carComponent.setLockon(false);
        throw new NotImplementedException();
    }

    void setPower(Message msg)
    {
        carComponent.setLockon(false);
        throw new NotImplementedException();
    }

    void setTurnRate(Message msg)
    {
        carComponent.setLockon(false);
        throw new NotImplementedException();
    }

    void startThrust(Message msg)
    {
        carComponent.setLockon(false);
        throw new NotImplementedException();
    }

    public void parseMessage(string message)
    {
        Message msg = JsonConvert.DeserializeObject<Message>(message);

        switch (msg.Type)
        {
            // Misc commands (same as doc)
            case "Username":
                username(msg);
                break;
            case "Color":
                color(msg);
                break;
            case "fullmap":
                fullmap(msg);
                break;
            case "allplayers":
                allplayers(msg);
                break;
            case "movetopoint":
                moveToPoints(msg);
                break;
            case "turnangle":
                turnAngle(msg);
                break;
            case "startThrust":
                startThrust(msg);
                break;
            case "setpower":
                setPower(msg);
                break;
            case "setturnrate":
                setTurnRate(msg);
                break;


            default:
                Debug.LogError("User sent command i do not know what is");
                Debug.LogError(message);

                Message invalid = new Message();
                invalid.Error("InvalidCommand","Unknown command sent");
                Send(JsonConvert.SerializeObject(invalid));

                break;
        }
    }

    internal void SendMapStatus()
    {
        Message data = new Message();
        data.MapStatus(server.mapStatusStr);
        Send(JsonConvert.SerializeObject(data));
    }

    internal void SendPlayerStatus()
    {
        string status = server.GeneratePlayerStatus(this);
        Message data = new Message();
        data.PlayerStatus()
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
#pragma warning disable CA2229 // Implement serialization constructors
public class InvalidCommandException : Exception
#pragma warning restore CA2229 // Implement serialization constructors
{
    public InvalidCommandException(string error) : base(error) { }
}