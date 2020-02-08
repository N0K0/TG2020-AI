using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class GameSocketServer : WebSocketServer
{
    public GameSocketServer(string url) : base(url)
    {

    }
}

public class Server : MonoBehaviour
{

    enum GameState : int {Main, Game, Done};

    private WebSocketServer wss = null;
    private List<CarController> players = null;
    private GameObject PlayerHolder = null;
    private GameObject car_prefab = null;
    // Used since i can only make game objects in the main thread
    private ConcurrentQueue<ClientController> clientsPending = new ConcurrentQueue<ClientController>();
    private float TickRate = 1 / 120; // 120 ticks per sec is the goal

    private GameState gameStatus = GameState.Main;


    [System.Obsolete] // TODO: Update the AddWebSocketService method to whatever is new and good
    void Start()
    {
        car_prefab = Resources.Load<GameObject>("CarController");
        DontDestroyOnLoad(gameObject);
        players = new List<CarController>();
        wss = new GameSocketServer("ws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
        wss.AddWebSocketService("/server", () => new ClientController(this) { } ) ; 
        wss.Start();
        InvokeRepeating("TickUpdate", 0f, TickRate);
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

        GameObject car_gameobject = Instantiate<GameObject>(car_prefab, PlayerHolder.transform);

        client.carController = car_gameobject;
        CarController carcontroller_component = car_gameobject.GetComponent<CarController>();
        carcontroller_component.server = this;

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

        if(gameStatus == GameState.Game)
        {
            GenerateMapStatus();
        }

        foreach (CarController car in players)
        {
            car.StartCoroutine("TickUpdate");
        }
    }

    void GenerateMapStatus()
    {

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