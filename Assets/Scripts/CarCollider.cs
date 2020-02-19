using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollider : MonoBehaviour
{
    CarController carController = null;

    public bool OnRoad;

    private void Update()
    {
        CheckRoad();
    }

    private void CheckRoad()
    {
        //public static bool Raycast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask);
        int layers =~ LayerMask.GetMask("Players");

        if (carController == null)
        {
            carController = gameObject.GetComponentInChildren<CarController>();
        }

        RaycastHit hit;
        Debug.DrawRay(gameObject.transform.position + new Vector3(0,20,0), Vector3.down * 100f, Color.cyan, 1f);
        bool RayHit = Physics.Raycast(gameObject.transform.position + new Vector3(0, 20, 0), Vector3.down, out hit, 100f, layers);
        if (RayHit)
        {
            OnRoad = hit.collider.gameObject.tag.Equals("Road");
            Debug.Log(hit.collider.gameObject.name + " " + OnRoad);
            carController.OnRoad(OnRoad);
        } else
        {
            Debug.Log("Ray did not hit anyting");
        }
    }
}
