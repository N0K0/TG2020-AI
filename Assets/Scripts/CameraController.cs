using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{


    public float RotationSpeed = 0.1f;
    public float MovementSpeed = 10f;
    GameObject[] cars;

    Vector3 camPos = new Vector3(0, 100, 100);

    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = new Vector2(0, 0);
        cars = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject car in cars)
        {
            pos += car.transform.position;
        }
        pos /= cars.Length;

        Vector3 targetPos = pos + camPos;

        Vector3 direction = (pos-transform.position ).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction);

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * MovementSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * RotationSpeed);
    }
}
