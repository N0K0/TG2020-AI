using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PathCreation;
using System;

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

    static readonly Color[] realColors = {
        Color.blue,
        Color.green,
        new Color(106,13,173),
        Color.red,
        new Color(192,192,192),
        Color.yellow
    };

    GameObject[] carPrefabs = new GameObject[6];
    
    public enum GameState : int {
        Main, 
        Game_Prelude,
        Game_Running,
        Done
    };

    public bool debug;

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

    public string mapStatusStr;
    internal GameState GameStatus;

    void Start()
    {
        GameStatus = GameState.Main;
        carModelNumber = UnityEngine.Random.Range(1, 7);

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
        wss = new GameSocketServer("xamws://localhost:8888");
        // Client controller has a callback to this class in which we instanceiate the gameopbject (must be done in main)
#pragma warning disable CS0618 // Type or member is obsolete
        wss.AddWebSocketService("/server", () => new ClientController(this) { } ) ;
#pragma warning restore CS0618 // Type or member is obsolete
        InvokeRepeating("TickUpdate", 0f, TickRate);
        wss.Start();
        Debug.Log("WSS Running");


    }

    internal void GameDone()
    {
        SceneManager.LoadScene("GameScene");
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

        // TODO: Clean up this mess
        GameObject car_gameobject = Instantiate<GameObject>(car_prefab);
        GameObject car_model = Instantiate<GameObject>(carPrefabs[prefabIndex]);
        DontDestroyOnLoad(car_model);

        car_gameobject.transform.SetParent(car_model.transform);
        car_model.transform.SetParent(PlayerHolder.transform);

        CarController carcontroller_component = car_gameobject.GetComponent<CarController>();

        client.carController = car_gameobject;
        
        carcontroller_component.server = this;
        client.carComponent = carcontroller_component;

        carcontroller_component.clientController = client;
        carcontroller_component.debug_color = realColors[players.Count];
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
        if (scene.name.Equals("GameScene"))
        {
            roundController.InitRound();
            GameStatus = GameState.Game_Prelude;
        } else if (scene.name.Equals("GameDone"))
        {
            GameStatus = GameState.Done;
            Debug.Log("GAME DONE!");
        }

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

        if (GameStatus == GameState.Game_Prelude)
        {
        }
        else if (GameStatus == GameState.Game_Running)
        {
        }
        else if (GameStatus == GameState.Main)
        {
        }

        foreach (CarController car in players.FindAll( car => car.Updateable))
        {
            car.StartCoroutine("TickUpdate");
        }
        Debug.Log("Server Tick Done");
    }

    void Update()
    {
        // Update car controllers
        ClientController client;
        if(clientsPending.TryDequeue(out client))
        {
            if (GameStatus != GameState.Main)
            {
                client.Close("Match is in progress!");
            }

            if (players.Count > 5) // Which equals six players
            {
                client.Close("Match is full!");
            } else
            {
                CreateCarControllerFromClient(client);
            }
        }
    }

    public string GeneratePlayerStatus(CarController player)
    {
        PlayerStatus ps = new PlayerStatus();

        Vector3 pos = player.transform.parent.position;
        Quaternion rot = player.transform.parent.rotation;

        ps.pos = pos;
        ps.rotation = rot.eulerAngles;

        // Find next checkpoint for player
        int index = player.GetNextCheckpointindex();

        Checkpoint checkpoint = roundController.checkpoints[index];

        ps.checkpoint_next_pos = checkpoint.transform.position;
        ps.checkpoint_next_rot = checkpoint.transform.rotation.eulerAngles;
        ps.thrustpower = player.thrustLevel;
        ps.turnrate = player.turnLevel;
        ps.thrustRemaining = player.thrustRemaining;
        ps.targetAngle = player.targetDir.y;

        ps.checkpointsHit = player.checkpointsHit;

        Debug.DrawLine(ps.pos, ps.checkpoint_next_pos, player.debug_color);

        return JsonConvert.SerializeObject(ps);
    }


    public string GenerateCompleteStatus()
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

        MapStatus mapStatus = new MapStatus();
        TrackController tc = GameObject.FindObjectOfType<TrackController>();
        RoadMeshCreator pst = GameObject.FindObjectOfType<RoadMeshCreator>();


        List<Vector3Json> bezierPoints = new List<Vector3Json>();
        
        foreach( Vector3 point in pst.pathCreator.bezierPath.points)
        {
            bezierPoints.Add(point);
        }

        mapStatus.bezierPoints = bezierPoints;
        mapStatus.roadWidth = pst.roadWidth;

        GameObject checkpointHolder = tc.checkpointHolder;
        List<Vector3Json> checkPointPos = new List<Vector3Json>();
        List<Vector3Json> checkPointRot = new List<Vector3Json>();

        Bounds checkPointBound = checkpointHolder.transform.GetChild(0).GetComponent<BoxCollider>().bounds;

        for (int i = 0; i < checkpointHolder.transform.childCount; i++)
        {
            Transform t = checkpointHolder.transform.GetChild(i);
            checkPointPos.Add(t.position);
            checkPointRot.Add(t.rotation.eulerAngles);
        }

        mapStatus.checkPointPos = checkPointPos;
        mapStatus.checkPointRot = checkPointRot;
        mapStatus.checkpointSize = checkPointBound.size;

        mapStatus.midpoint = new List<Vector3Json>();
        mapStatus.wallLeft = new List<Vector3Json>();
        mapStatus.wallRight = new List<Vector3Json>();
        mapStatus.roadDirection = new List<Vector3Json>();

        VertexPath vertexPath = pst.pathCreator.path;

        Vector3[] pathPoints = vertexPath.localPoints;

        for(int i = 0; i < vertexPath.localPoints.Length; i++)
        {
            Vector3 midPoint = pathPoints[i];
            Vector3 localRight = Vector3.Cross(Vector3.up, vertexPath.GetTangent(i));
            
            Vector3 vertSideA = vertexPath.GetPoint(i) - localRight * Mathf.Abs(pst.roadWidth);
            Vector3 vertSideB = vertexPath.GetPoint(i) + localRight * Mathf.Abs(pst.roadWidth);

            mapStatus.midpoint.Add(midPoint);
            mapStatus.wallLeft.Add(vertSideA);
            mapStatus.wallRight.Add(vertSideB);
            mapStatus.roadDirection.Add(vertexPath.GetTangent(i));

        }

        string mapStatusJson = JsonConvert.SerializeObject(mapStatus);
        //Debug.Log(mapStatusJson);
        
        if (debug)
        {
            for (int i = 1; i < mapStatus.wallLeft.Count; i++)
            {
                // Left
                Vector3 point_l1 = mapStatus.wallRight[i - 1];
                Vector3 point_l2 = mapStatus.wallRight[i];
                Debug.DrawLine(point_l1, point_l2, Color.red);

                // Right
                Vector3 point_r1 = mapStatus.wallLeft[i - 1]; 
                Vector3 point_r2 = mapStatus.wallLeft[i];
                Debug.DrawLine(point_r1, point_r2, Color.blue);
            }
        }

        return mapStatusJson;
    }
}


class PlayerStatus
{
    /*
     * This class covers the facts needed for a single player
     * 
     * Since we don't have collisions on in this game we don't need to send a lot of information
     * 
     */

    public bool[] checkpointsHit;
    public Vector3Json pos;
    public Vector3Json rotation;
    public Vector3Json checkpoint_next_pos;
    public Vector3Json checkpoint_next_rot;

    public float thrustpower;
    public float turnrate;

    public float targetAngle;
    public float thrustRemaining;
}

class MapStatus
{
    /*
     * This class covers the needed facts for a single player racing through the track
     * The road definition, and the checkpoint definitions
     * 
     * TODO: Explain users what the fuck an Quaternion really is
     */

    public List<Vector3Json> bezierPoints; // TODO: Explain users what an bezier curve is..
    public float roadWidth;

    public List<Vector3Json> checkPointPos; 
    public List<Vector3Json> checkPointRot;
    public Vector3Json checkpointSize; // Just fetch the bounds of the collider

    public List<Vector3Json> midpoint;
    public List<Vector3Json> wallLeft;
    public List<Vector3Json> wallRight;
    public List<Vector3Json> roadDirection;
}

class Vector3Json
{
    public float x, y, z;

    public static implicit operator Vector3Json(Vector3 vec)
    {
        return new Vector3Json(vec);
    }

    public static implicit operator Vector3(Vector3Json vec)
    {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    Vector3Json(Vector3 vec)
    {
        x = vec.x;
        y = vec.y;
        z = vec.z;
    }
}

class QuaternionJson
{
    public float w, x, y, z;

    public static implicit operator QuaternionJson(Quaternion vec)
    {
        return new QuaternionJson(vec);
    }

    public static implicit operator Quaternion(QuaternionJson vec)
    {
        return new Quaternion(vec.x, vec.y, vec.z, vec.w);
    }

    QuaternionJson(Quaternion quart)
    {
        w = quart.w;
        x = quart.x;
        y = quart.y;
        z = quart.z;
    }
}


class Message
{

    /* Will try to use tihs class as a storage for all the normal outgoing/incoming messages for the clients.
     * That is, basic is defined as the commands simply containing a type and a simple string
     */

    public string Type = "INVALID COMMAND. NIKOLAS DUN GOOFED. SLAP HIM"; // Some fun is allowed :D
    public string Status  = "OK"; // Lets assume ok unless the opposite is stated
    public string Command = "";

    public void RequestUsername() // Same Type is used for setting
    {
        Type = "username";
        Status = "OK"; 
        Command = "Username OK";
    }

    public void MapStatus(string serializedMapData)
    {
        Type = "fullMap";
        Status = "OK";
        Command = serializedMapData;
    }


    internal void PlayerStatus(string serializedMapData)
    {
        Type = "playerStatus";
        Status = "OK";
        Command = serializedMapData;
    }

    public void Error(string type, string command)
    {
        Type = type;
        Command = command;
    }
}
