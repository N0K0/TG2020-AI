using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
        parseMessage(e.Data);
    }

    protected override void OnOpen()
    {
        Debug.Log("New connection open! " + this.Sessions.Count);
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

        Message error = new Message();
        error.Error(error.Type, e.Message);
        Send(JsonConvert.SerializeObject(error));
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

    void moveToPoint(Message msg)
    {
        carComponent.setLockon(true);
        JToken token = JObject.Parse(msg.Command);
        float x = token.SelectToken("x").Value<float>();
        float z = token.SelectToken("z").Value<float>();

        //Debug.Log(msg.Command);
        // This function is a bit special, 
        // its to help players that don't want to do everything on their own.
        carComponent.moveToPoint(x, z);
    }

    void turnAngle(Message msg)
    {
        carComponent.setLockon(false);
        JToken token = JObject.Parse(msg.Command);
        float angle = token.SelectToken("angle").Value<float>();

        // The target angle relativ to the world
        carComponent.turnAngle(angle);
    }

    void setPower(Message msg)
    {
        carComponent.setLockon(false);
        JToken token = JObject.Parse(msg.Command);
        float value = token.SelectToken("value").Value<float>();
        carComponent.setPower(value);
    }

    void setTurnRate(Message msg)
    {
        carComponent.setLockon(false);
        JToken token = JObject.Parse(msg.Command);
        float value = token.SelectToken("value").Value<float>();
        carComponent.setTurnRate(value);
    }

    void startThrust(Message msg)
    {
        carComponent.setLockon(false);
        JToken token = JObject.Parse(msg.Command);
        float value = token.SelectToken("value").Value<float>();
        carComponent.startThrust(value);
    }

    public void parseMessage(string message)
    {

        JsonSerializerSettings settings = new JsonSerializerSettings();
        settings.MaxDepth = 1;

        JToken token = JObject.Parse(message);
        Message msg = new Message();
        msg.Type = token.SelectToken("Type").Value<string>();
        msg.Command = token.SelectToken("Command").ToString();

        // Debug.Log(msg.Command);
        switch (msg.Type)
        {
            // Misc commands (same as doc)
            case "Username":
                username(msg);
                break;
            case "Color":
                color(msg);
                break;
            case "fullMap":
                fullmap(msg);
                break;
            case "allPlayers":
                allplayers(msg);
                break;
            case "moveToPoint":
                moveToPoint(msg);
                break;
            case "turnAngle":
                turnAngle(msg);
                break;
            case "startThrust":
                startThrust(msg);
                break;
            case "setPower":
                setPower(msg);
                break;
            case "setTurnRate":
                setTurnRate(msg);
                break;

            default:
                UnityEngine.Debug.LogError("User sent command i do not know what is");
                UnityEngine.Debug.LogError(message);

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
        string status = server.GeneratePlayerStatus(this.carComponent);
        Message data = new Message();
        data.PlayerStatus(status);
        Send(JsonConvert.SerializeObject(data));
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
        //Debug.Log(msg);
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