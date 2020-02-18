using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollider : MonoBehaviour
{

    CarController carController = null;

    private void OnCollisionEnter (Collision collision)
    {
        if(carController == null)
        {
            carController = gameObject.GetComponentInChildren<CarController>();
        }
        carController.OnRoad(true);
    }

    private void OnCollisionExit(Collision collision)
    {

        if (carController == null)
        {
            carController = gameObject.GetComponentInChildren<CarController>();
        }
        carController.OnRoad(false);
    }

    private void Update()
    {
        CheckRoad();
    }

    private void CheckRoad()
    {
        RaycastHit hit;
        if (Physics.Raycast(gameObject.transform.position, Vector3.down, out hit))
        {
            Debug.Log(hit.collider.gameObject.name);
        }
    }
}
