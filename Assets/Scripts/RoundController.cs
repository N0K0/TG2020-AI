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
            car.Freeze = freeze;
        }
    }

    void UnfreezePlayers()
    {
        FreezePlayers(false);
    }


    public IEnumerator CountDownToStart()
    {
        currCountdownValue = countDownTime;
        while (currCountdownValue > 0)
        {
            Debug.Log("Countdown: " + currCountdownValue);
            yield return new WaitForSeconds(1.0f);
            currCountdownValue--;
        }
        UnfreezePlayers();
        ui.SetActive(false);
    }

    internal void InitRound()
    {
        playerHolder = GameObject.Find("PlayerHolder");
        ui = GameObject.Find("UI");
        Text text = ui.transform.GetChild(0).transform.GetComponent<Text>();
        text.text = "Countdown to start: " + currCountdownValue.ToString();

        foreach ( CarController car in server.GetPlayers())
        {
            cars.Add(car.gameObject);
        }
        StartCoroutine("CountDownToStart");
    }
}
