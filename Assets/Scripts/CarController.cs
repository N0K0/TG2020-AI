using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;


public class CarController : MonoBehaviour
{

    public ClientController clientController = null;
    public Server server = null;
    public string UserName = "Anon";

    public bool Playable = true; // Used to check is a player has been DQed. Can't remove the object before after a round is done due to score
    public bool Active = true;
    internal bool Updateable = true;
    internal bool Freeze = true; // Used to lock players at start of round

    public bool[] checkpointsHit; // This bool contains the hits

    // The stuff that makes the car go wroom
    private Vector3 targetAngle;
    private Vector3 targetThrust;

    private bool pointLockon; // This is active

    private float thrustLevel; // Used to regulate the turnrate and how big of an impulse that is outputted
    private float turnLevel;

    void TickUpdate() {
        Debug.Log("Running Tick update in car: " + UserName);

        // Breaking up the tests into which state the server has
        switch( server.GameStatus)
        {
            case Server.GameState.Main:
                if (UserName.Equals("Anon"))
                {
                    clientController.RequestUsername();
                }

                break;

            case Server.GameState.Game_Prelude:
            case Server.GameState.Game_Running:
                string status = server.GeneratePlayerStatus(this);
                break;

            default:
                Debug.LogError("Tick update hit wrongful state");
                break;
        }


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
        Updateable = false;
        clientController.Close("CarController stopped");
    }

    public void OnDestroy()
    {
        clientController.Close("CarController was destroyed");
    }

    public void SetUsername(string username)
    {
        // Lets just ignore the possible racecondition and let each user check the server table on its own.
        // Aka, i have no god damn clue how Websocket-sharp threads anything
        //
        // Edit: Apparently neither does the rest of the community

        // Username can't be blank
        if(username.Length <= 0)
        {
            throw new InvalidCommandException("Username can not be empty");
        }

        // Username can't be Anon
        if (username.Equals("Anon"))
        {
            throw new InvalidCommandException("Anon is not a valid username");
        }

        // Game can't be live
        if (server.GameStatus != Server.GameState.Main)
        {
            throw new InvalidCommandException("You can not change name when game is live");
        }

        // Username can't be an others username
        foreach ( CarController car in server.GetPlayers() ) // No need to check invalid players
        {
            if (car.UserName.Equals(username))
            {
                throw new InvalidCommandException("Username is already in use");
            }
        }

        UserName = username; // Aiight. Nothing wrong thus far
    }

    public bool getLockon()
    {
        return pointLockon;
    }

    public void setLockon(bool val)
    {
        pointLockon = val;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger event for car: " + this.UserName);
    }

    internal void SendMapStatus()
    {
        clientController.SendMapStatus();
    }
}
