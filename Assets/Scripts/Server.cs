using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class GameSocketServer : WebSocketServer
{
    public GameSocketServer(string url) : base(url)
    {

    }
}

public class Server : MonoBehaviour
{

    AsyncOperation asyncLoadLevel;

    public enum GameState : int {Main, Game, Done};

    private WebSocketServer wss = null;
    private List<CarController> players = null;
    private GameObject PlayerHolder = null;
    private GameObject car_prefab = null;

    private RoundController roundController = null;

    // Used since i can only make game objects in the main thread
    private ConcurrentQueue<ClientController> clientsPending = new ConcurrentQueue<ClientController>();
    private float TickRate = 1f / 120f; // 120 ticks per sec is the goal

    internal GameState gameStatus = GameState.Main;

    [System.Obsolete] // TODO: Update the AddWebSocketService method to whatever is new and good
    void Start()
    {
        roundController = GameObject.Find("RoundController").GetComponent<RoundController>();
        roundController.server = this;


        car_prefab = Resources.Load<GameObject>("CarController");
        DontDestroyOnLoad(gameObject);
        players = new List<CarController>();
        wss = new GameSocketServer("ws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
        wss.AddWebSocketService("/server", () => new ClientController(this) { } ) ;
        InvokeRepeating("TickUpdate", 0f, TickRate);
        wss.Start();
        Debug.Log("WSS Running");

    }

    public void RegisterClient(ClientController client)
    {
        // Needed since CarController must be made in the main thread
        clientsPending.Enqueue(client);
    }
    public void UnregisterClient(ClientController client)
    {

    }

    public void KickPlayer(CarController car) {
        car.StopPlayer();
    }


    public void CreateCarControllerFromClient(ClientController client)
    {
        if(PlayerHolder == null )
       {
            print("Playerholder was empty");
            PlayerHolder = new GameObject("PlayerHolder");
            Instantiate(PlayerHolder);
        }
        else
        {
            print("Playerholder search");
            PlayerHolder = GameObject.Find("PlayerHolder");
        }

        if(PlayerHolder == null)
        {
            print("Playerholder search was empty");
            PlayerHolder = new GameObject("PlayerHolder");
            Instantiate(PlayerHolder);
        }

        DontDestroyOnLoad(PlayerHolder);

        // TODO: Clean up this mess
        GameObject car_gameobject = Instantiate<GameObject>(car_prefab, PlayerHolder.transform);
        client.carController = car_gameobject;
        CarController carcontroller_component = car_gameobject.GetComponent<CarController>();
        carcontroller_component.server = this;
        client.carComponent = carcontroller_component;

        carcontroller_component.clientController = client;
        players.Add(carcontroller_component);
    }

    public List<CarController> GetPlayers()
    {
        List<CarController> lst = new List<CarController>();
        
        foreach ( CarController car in players)
        {
            if(car.Active && car.Playable)
            {
                lst.Add(car);
            }
        }
        return lst;
    }

    public void StartServer()
    {
        // Lets purge all cars which should not be with into the game
        PurgeInvalidPlayers();

        if (players.Count == 0)
        {
            Debug.LogWarning("No players left after purge..");
            // TODO: UI box on screen?
            return;
        }


        // Lets load the next scene
        SceneManager.LoadScene("GameScene");
        // In the next scene we will also have the RoundController

        SceneManager.sceneLoaded += GameSceneLoaded;

    }

    void GameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        roundController.InitRound();
        SceneManager.sceneLoaded -= GameSceneLoaded; // Need this as a oneshot only
    }

    private static bool NotValidPlayer(CarController car)
    {
        return !ValidPlayer(car);
    }

    private static bool ValidPlayer(CarController car)
    {
        return car.Playable && car.Active && !car.UserName.Equals("Anon");
    }

    public void PurgeInvalidPlayers()
    {
        foreach ( CarController car in players)
        {
            if (!ValidPlayer(car))
            {
                car.KickPlayer();
            }
        }
        players.RemoveAll(NotValidPlayer);
    }

    void TickUpdate()
    {
        Debug.Log("Server Tick");

        if (gameStatus == GameState.Game)
        {
        }
        else if (gameStatus == GameState.Main)
        {
        }

        foreach (CarController car in players.FindAll( car => car.Updateable))
        {
            car.StartCoroutine("TickUpdate");
        }
        Debug.Log("Server Tick Done");
    }

    void GenerateCompleteStatus()
    {
        /* This funciton creates the general status of the map all the players need
         * 
         * Mapstatus should contain the following:
         * 
         * 1) The Bezier definition of the track
         * 2) The Vertext definition of the walls of the track
         * 3) Placement of all the checkpoints
         * 
         */
    }

    void GeneratePlayerStatus()
    {
        //
    }

    void Update()
    {
        // Update car controllers
        ClientController client;
        if(clientsPending.TryDequeue(out client))
        {
            if(players.Count > 5) // Which equals six players
            {
                client.Close("Match is full!");
            } else
            {
                CreateCarControllerFromClient(client);
            }
        }
    }
}

class MapStatus
{
    /*
     * This class covers ass the needed facts for a single player racing through the track
     */
    
}

class Message
{

    /* Will try to use tihs class as a storage for all the normal outgoing messages for the clients.
     * That is, basic is defined as the commands simply containing a type and a simple string
     */

    public string Type = "INVALID COMMAND. NIKOLAS DUN GOOFED"; // Some fun is allowed :D
    public string Status  = "OK"; // Lets assume ok unless the opposite is stated
    public string Command = "";

    [JsonExtensionData] // This is where all the extra data ends up during parsing
    public IDictionary<string, JToken> Extras = null;

    public void RequestUsername() // Same Type is used for setting
    {
        Type = "username";
        Status = "OK"; 
        Command = "Username OK";
    }
}
