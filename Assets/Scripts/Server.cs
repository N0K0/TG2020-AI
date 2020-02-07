using UnityEngine;
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
    private WebSocketServer wss = null;
    private WebSocketServiceHost serviceHost = null;

    private List<CarController> players = null;

    public GameObject PlayerHolder = null;

    private GameObject car_prefab = null;
    // Used since i can only make game objects in the main thread
    private ConcurrentQueue<ClientController> clientsPending = new ConcurrentQueue<ClientController>();

    private void Start()
    {
        car_prefab = Resources.Load<GameObject>("CarController");
    }

    [System.Obsolete] // TODO: Update the AddWebSocketService method to whatever is new and good
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        players = new List<CarController>();
        wss = new GameSocketServer("ws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
        wss.AddWebSocketService("/server", () => new ClientController(this) { } ) ; 
        wss.Start();
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
        serviceHost = wss.WebSocketServices["/server"];
        WebSocketSessionManager clients = serviceHost.Sessions;
    }

    void Update()
    {
        // Update car controllers
        ClientController client;
        if(clientsPending.TryDequeue(out client))
        {
            CreateCarControllerFromClient(client);
        }
        
    }
}