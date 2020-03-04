using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoundController : MonoBehaviour
{
    public Server server = null;
    GameObject playerHolder = null;
    List<GameObject> cars = null;
    GameObject ui = null;

    public GameObject checkPointHolder = null;
    public Checkpoint[] checkpoints = null;

    float countDownTime = 10;
    float currCountdownValue = 10;

    float countDownTimeGameDone = 120;
    float currCountdownValueGameDone = 120;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        cars = new List<GameObject>();
    }

    void FreezePlayers(bool freeze)
    {
        TrackController tc = GameObject.Find("MapController").GetComponent<TrackController>();
        Vector3 startPos = tc.getStartPos() + new Vector3(0, 5, 0);
        Quaternion startRotation = tc.getStartRotation() * Quaternion.Euler(90, 0, 90);

        foreach ( CarController car in server.GetPlayers())
        {

            car.Freeze = freeze; // I might be retarded. This is the same as Is Kinematic
            Rigidbody r = car.GetComponentInParent<Rigidbody>();
            r.isKinematic = freeze;

            if(freeze)
            {
                r.constraints = RigidbodyConstraints.FreezeAll;
            } else
            {
                r.constraints = RigidbodyConstraints.None;
                car.transform.position = startPos;
                car.transform.rotation = startRotation;
            }

        }
    }

    internal void notifyHit(CarController carController, Checkpoint checkpoint)
    {
        Debug.Log("Notify hit (" + checkpoint.getId() + ") "+ carController.UserName);
        // TODO: CHECK IS THIS IS THE CORRECT CHECKPOINT

        int index = carController.GetNextCheckpointindex();
        if(checkpoint.getId() == index)
        {
            Debug.Log("Next Checkpoint hit!");
            carController.checkpointsHit[index] = true;
        }

       if( index == carController.checkpointsHit.Length -1)
        {
            notifyDone(carController);
        }
        
    }

    internal void notifyDone(CarController carController)
    {
        carController.DoneWithRace = true;
        StartCoroutine("RoundDoneCountDown");
        bool done = true;
        foreach (CarController car in server.GetPlayers())
        {
            if (!car.DoneWithRace)
            {
                done = false;
                break;
            }
        }

        if (done)
        {
            server.GameDone();
        }
    }



    void UpdateLeaderBoard()
    {
        /*
         * Used to control the status of the UI
         * The Ranking based on checkpoints and potential contdowns
         * 
         * TODO: Look into using "Get closest point to path" from the track generator
         */
    }

    void UnfreezePlayers()
    {
        FreezePlayers(false);
    }

    public IEnumerator CountDownToStart()
    {
        ui = GameObject.Find("CountdownText");
        Text text = ui.transform.GetChild(0).transform.GetComponent<Text>();

        currCountdownValueGameDone = countDownTimeGameDone;
        while (currCountdownValueGameDone > 0)
        {
            text.text = "Countdown to round done: " + currCountdownValueGameDone.ToString() + " sec";
            //Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValueGameDone--;
        }
        server.GameDone();
    }

    internal IEnumerator RoundDoneCountDown()
    {
        {
            ui = GameObject.Find("CountdownText");
            Text text = ui.transform.GetChild(0).transform.GetComponent<Text>();

            currCountdownValue = countDownTime;
            while (currCountdownValue > 0)
            {
                text.text = "Countdown: " + currCountdownValue.ToString() + " sec";
                //Debug.Log("Countdown: " + currCountdownValue);
                yield return new WaitForSeconds(1.0f);
                currCountdownValue--;
            }
            UnfreezePlayers();
            ui.SetActive(false);
            server.GameStatus = Server.GameState.Game_Running;
        }
    }

    internal void InitRound()
    {
        // Start figuring out the checkpoints and set the pos of the players

        playerHolder = GameObject.Find("PlayerHolder");
        GameObject cps = GameObject.Find("Checkpoints");

        checkpoints = cps.GetComponentsInChildren<Checkpoint>();

        TrackController tc = GameObject.Find("MapController").GetComponent<TrackController>();

        Vector3 startPos = tc.getStartPos() + new Vector3(0, 5, 0);
        Quaternion startRotation = tc.getStartRotation() * Quaternion.Euler(90, 0, 90);

        string mapStatusStr = server.GenerateCompleteStatus();
        server.mapStatusStr = mapStatusStr;
        // Send players the mapdata
        foreach ( CarController car in server.GetPlayers())
        {

            cars.Add(car.gameObject);
            car.checkpointsHit = new bool[checkpoints.Length];
            car.transform.parent.transform.position = startPos;
            car.transform.parent.rotation = startRotation;

            car.SendMapStatus();
        }

        FreezePlayers(true);

        for (int i = 0; i < checkpoints.Length; i++)
        {
            checkpoints[i].setId(i);
        }

        StartCoroutine("CountDownToStart");
    }
}
