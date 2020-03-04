using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingsHolder : MonoBehaviour
{

    /* Road things */
    public float thrustLevelMax = 30f; // Used to regulate the turnrate and how big of an impulse that is outputted
    public float turnLevelMax = 30f;
    public float velLevelMax = 30f;

    public float thrustLevelMaxRoad = 100f; // Used to regulate the turnrate and how big of an impulse that is outputted
    public float turnLevelMaxRoad = 60f;
    public float velLevelMaxRoad = 90f;


    /* Map things */
    public int mapPointsMin = 3;
    public int mapPointsMax = 10;

    /* Camera things */
    public float RotationSpeed = 0.1f;
    public float MovementSpeed = 10f;


    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }
}
