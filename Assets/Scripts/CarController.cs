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

    // Start is called before the first frame update
    public void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void TickUpdate() {
        Debug.Log("Running Tick update in car: " + UserName);
        if( server.gameStatus == Server.GameState.Main && UserName.Equals("Anon"))
        {
            clientController.RequestUsername();
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

    internal void SetUsername(string username)
    {
        // Lets just ignore the possible racecondition and let each user check the server table on its own.
        // Aka, i have no god damn clue how Websocket-sharp threads anything
        //
        // Edit: Apparently neither does the rest of the community



        foreach
    }
}
