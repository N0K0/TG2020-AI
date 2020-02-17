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
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setId(int num)
    {
        id = num;
    }

    public void setRoundController(RoundController x)
    {
        rc = x;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CarController cc = other.gameObject.GetComponent<CarController>();
            if(cc == null)
            {
                Debug.LogError("Hit car prefab without CarController");
            }
            rc.notifyHit(cc);
        } else
        {
            Debug.Log("Hit trigger not related to Player\n\t" + other.gameObject.name);
        }
    }
}
