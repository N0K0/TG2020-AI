using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using Newtonsoft.Json;


public class CarController : MonoBehaviour
{

    public SettingsHolder sh = null;
    public ClientController clientController = null;
    public Server server = null;
    public string UserName = "Anon";

    public bool Playable = true; // Used to check is a player has been DQed. Can't remove the object before after a round is done due to score
    public bool Active = true;
    internal bool Updateable = true;
    internal bool Freeze = true; // Used to lock players at start of round


    internal Color debug_color;

    Vector3 pos;
    Quaternion rot;

    public bool[] checkpointsHit; // This bool contains the hits

    // The stuff that makes the car go wroom
    public Vector3 targetDir; // Relative to world
    public float thrustRemaining; // thrust remaining


    private bool pointLockon; // This is active
    public Vector3 targetPoint; // The point we are trying to move towards

    public float thrustLevel; // Used to regulate the turnrate and how big of an impulse that is outputted
    public float turnLevel;

    private float thrustLevelMax; // Used to regulate the turnrate and how big of an impulse that is outputted
    private float turnLevelMax;
    private float velLevelMax;

    private float thrustLevelMaxRoad; // Used to regulate the turnrate and how big of an impulse that is outputted
    private float turnLevelMaxRoad;
    private float velLevelMaxRoad;

    private float maxSpeed;
    private float maxSpeedRoad;


    public bool onRoad;
    public bool DoneWithRace = false;

    void Start()
    {

        sh = GameObject.Find("SettingsHolder").GetComponent<SettingsHolder>();

        targetDir = Vector3.forward;
        targetPoint = Vector3.zero;

        thrustLevel = sh.settings.thrustLevelMaxRoad;
        turnLevel = sh.settings.turnLevelMax;

        thrustLevelMax = sh.settings.thrustLevelMax; // Used to regulate the turnrate and how big of an impulse that is outputted
        turnLevelMax = sh.settings.turnLevelMax;
        velLevelMax = sh.settings.velLevelMax;

        thrustLevelMaxRoad = sh.settings.thrustLevelMaxRoad;
        turnLevelMaxRoad = sh.settings.turnLevelMaxRoad;
        velLevelMaxRoad = sh.settings.velLevelMaxRoad;
    }

    void Update()
    {
        pos = transform.parent.position;
        rot = transform.parent.rotation;

        float thrustMax;
        float turnMax;
        float velMax;

        if (onRoad)
        {
            thrustMax = thrustLevelMaxRoad;
            turnMax =   turnLevelMaxRoad;
            velMax =    velLevelMaxRoad;
        } else
        {
            thrustMax = thrustLevelMax;
            turnMax = turnLevelMax;
            velMax = velLevelMax;
        }

        // This is where we actually need to be to be able to apply force and the likes
        Debug.DrawLine(targetPoint, pos, Color.black, 1f);

        if (server.GameStatus == Server.GameState.Game_Running)
        {
            Rigidbody rigidbody = gameObject.GetComponentInParent<Rigidbody>();
            rigidbody.freezeRotation = true;
            rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, velMax);

            if (pointLockon) // This bool is flipped based on move to point vs manually setting angles
            {
                float turn = Mathf.Clamp(turnLevel, 0.001f, turnMax);
                Quaternion targetRotation = Quaternion.LookRotation(targetPoint - pos, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
                float singleStep = turn * Time.deltaTime;
                transform.parent.rotation = Quaternion.RotateTowards(rot, targetRotation, singleStep) ;
            }
            else
            {
                float turn = Mathf.Clamp(turnLevel, 0.001f, turnMax);
                Quaternion targetRotation = Quaternion.LookRotation(targetDir, Vector3.up) * Quaternion.Euler(0f, -90f, 0f);
                float singleStep = turn * Time.deltaTime;
                transform.parent.rotation = Quaternion.RotateTowards(rot, targetRotation, singleStep);
            }

            if (thrustRemaining > 0)
            {

                float thrust = Mathf.Clamp(thrustLevel, 0.01f, thrustMax);
                Debug.DrawRay(pos, transform.parent.right * 10, Color.red, 2f);
                thrustRemaining -= Time.deltaTime;
                rigidbody.AddForce(transform.parent.right * thrustLevel * 1, ForceMode.Acceleration);
            }
        }
    }


    void TickUpdate() {
        //Debug.Log("Running Tick update in car: " + UserName);

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
                clientController.SendPlayerStatus();
                break;

            case Server.GameState.Done:
                Debug.Log("The Game is done");
                break;

            default:
                Debug.LogError("Tick update hit wrongful state");
                break;
        }
        return;
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

    internal void setTurnRate(float value)
    {
        float power = Mathf.Clamp(value, 1f, turnLevelMaxRoad);
        turnLevel = power;
    }

    internal void startThrust(float value)
    {
        // The value is number of sec we want to thrust
        thrustRemaining = value;
    }

    internal void setPower(float value)
    {
        float power = Mathf.Clamp(value, 1f, thrustLevelMaxRoad);
        thrustLevel = power;
    }

    internal void turnAngle(float angle)
    {
        Debug.Log("Turn angle: " + angle);
        targetDir = DirFromAngle(angle);
    }

    internal void turnAngleRelative(float angle)
    {
        targetDir = Quaternion.Euler(0f, angle, 0f) * targetDir;
        Debug.Log(angle);
        Debug.Log(targetDir);
    }

    public Vector3 DirFromAngle(float angleInDegree)
    {
        return new Vector3(Mathf.Sin(angleInDegree * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegree * Mathf.Deg2Rad));
    }

    public void moveToPoint(float x, float z)
    {
        //Debug.Log(string.Format("X: {0} Z: {1}",x,z));
        targetPoint = new Vector3(x, pos.y, z);
        startThrust(1.0f);
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

    internal void OnRoad(bool v)
    {
        Debug.Log(UserName + " Road status " + v);
        onRoad = v;
    }

    internal void SendMapStatus()
    {
        clientController.SendMapStatus();
    }

    public int GetNextCheckpointindex()
    {
        int index;
        for (index = 0; index < checkpointsHit.Length; index++)
        {
            if (checkpointsHit[index] == false)
            {
                break;
            }
        }
        return index;
    }
}
