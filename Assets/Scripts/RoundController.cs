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

    GameObject checkPointHolder = null;
    List<GameObject> checkpoints = null;

    float countDownTime = 10;
    float currCountdownValue = 10;

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        cars = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FreezePlayers(bool freeze)
    {
        foreach( CarController car in server.GetPlayers())
        {
            car.Freeze = freeze; // I might be retarded. This is the same as Is Kinematic
            car.GetComponentInChildren<Rigidbody>().isKinematic = freeze;
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
        ui = GameObject.Find("UI");
        Text text = ui.transform.GetChild(0).transform.GetComponent<Text>();

        currCountdownValue = countDownTime;
        while (currCountdownValue > 0)
        {
            text.text = "Countdown: " + currCountdownValue.ToString() + " sec";
            Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        UnfreezePlayers();
        ui.SetActive(false);
        server.gameStatus = Server.GameState.Game_Running;
    }

    internal void InitRound()
    {
        // Start figuring out the checkpoints and set the pos of the players

        playerHolder = GameObject.Find("PlayerHolder");
        checkPointHolder = GameObject.Find("Checkpoints");
        TrackController tc = GameObject.Find("MapController").GetComponent<TrackController>();

        Vector3 startPos = tc.getStartPos() + new Vector3(0, 5, 0);
        Quaternion startRotation = tc.getStartRotation() * Quaternion.Euler(90, 0, 90);

        // Send players the mapdata

        foreach ( CarController car in server.GetPlayers())
        {   
            cars.Add(car.gameObject);
            car.transform.position = startPos;
            car.transform.rotation = startRotation;
        }

        StartCoroutine("CountDownToStart");
    }
}
