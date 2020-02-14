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
    static readonly string[] carColors = {
        "Blue",
        "Green",
        "Purple",
        "Red",
        "Silver",
        "Yellow"
    };

    GameObject[] carPrefabs = new GameObject[6];
    
    public enum GameState : int {
        Main, 
        Game_Prelude,
        Game_Running,
        Done
    };

    readonly string carPath = "LowPolyCarPack/Prefabs/Car_{0}_{1}";

    private WebSocketServer wss = null;
    private List<CarController> players = null;
    private GameObject PlayerHolder = null;
    private GameObject car_prefab = null;

    private int carModelNumber = 1;

    private RoundController roundController = null;

    // Used since i can only make game objects in the main thread
    private ConcurrentQueue<ClientController> clientsPending = new ConcurrentQueue<ClientController>();
    private float TickRate = 1f / 120f; // 120 ticks per sec is the goal

    internal GameState gameStatus = GameState.Main;

    void Start()
    {

        carModelNumber = Random.Range(1, 7);

        float start = Time.unscaledTime;
        for( int i = 0; i < 6; i++)
        {
            string modelPath = string.Format(carPath, carModelNumber, carColors[i]);
            Debug.Log("Loading: " + modelPath);
            carPrefabs[i] = Resources.Load<GameObject>(modelPath);
        }

        Debug.Log("Load models time: " + (Time.unscaledTime - start));

        roundController = GameObject.Find("RoundController").GetComponent<RoundController>();
        roundController.server = this;

        car_prefab = Resources.Load<GameObject>("CarController");
        DontDestroyOnLoad(gameObject);
        players = new List<CarController>();
        wss = new GameSocketServer("ws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
#pragma warning disable CS0618 // Type or member is obsolete
        wss.AddWebSocketService("/server", () => new ClientController(this) { } ) ;
#pragma warning restore CS0618 // Type or member is obsolete
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

        int prefabIndex = PlayerHolder.transform.childCount;
        Debug.Log(prefabIndex);

        // TODO: Clean up this mess
        GameObject car_gameobject = Instantiate<GameObject>(car_prefab, PlayerHolder.transform);
        client.carController = car_gameobject;
        CarController carcontroller_component = car_gameobject.GetComponent<CarController>();
        GameObject car_model = Instantiate<GameObject>(carPrefabs[prefabIndex], car_gameobject.transform);
        
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
        GenerateCompleteStatus();


        SceneManager.sceneLoaded -= GameSceneLoaded; // Need this as a oneshot only
        gameStatus = GameState.Game_Prelude;
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

        if (gameStatus == GameState.Game_Prelude)
        {
        }
        else if (gameStatus == GameState.Game_Running)
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
         * and sends it to them
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

    public string Type = "INVALID COMMAND. NIKOLAS DUN GOOFED. SLAP HIM"; // Some fun is allowed :D
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
