using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{

    RoundController rc;
    public Server server = null;
    int id;

    // Start is called before the first frame update
    void Start()
    {
        GameObject obj = GameObject.Find("RoundController");
        rc = obj.GetComponent<RoundController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setId(int num)
    {
        id = num;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameObject obj = other.gameObject;
            CarController cc = obj.GetComponentInChildren<CarController>();
            if(cc == null)
            {
                Debug.LogError("Hit car prefab without CarController");
                return;
            }
            rc.notifyHit(cc, this);
        } else
        {
            Debug.Log("Hit trigger not related to Player\n\t" + other.gameObject.name);
        }
    }

    internal int getId()
    {
        return id;
    }
}
