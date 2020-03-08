using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class SettingsHolder : MonoBehaviour
{

    public Settings settings;
    public RectTransform ui_buttons;
    public RectTransform ui_val;
    public Hashtable setMap = new Hashtable();



    void Start()
    {
        settings = new Settings();
        DontDestroyOnLoad(this.gameObject);
        connectElements();
    }

    public void connectElements()
    {
        Debug.Log("Connecting elements");
        Button[] buttons = ui_buttons.GetComponentsInChildren<Button>();
        InputField[] inputs = ui_val.GetComponentsInChildren<InputField>();
        int childNum = buttons.Length;
        Debug.Log(childNum);
        for (int i = 0; i < childNum; i++)
        {
            Button btn = buttons[i];
            InputField input = inputs[i];
            FieldInfo field = settings.GetType().GetField(btn.name);
            UISettingPair pair = new UISettingPair(btn.name, btn, input, field, settings);
            setMap.Add(btn.name, pair);
            Debug.Log(field);
        }
    }

    public void refresh()
    {
        foreach(DictionaryEntry entry in setMap)
        {
            var val = (UISettingPair)entry.Value;
            val.refreshValue();
        }
    }
}

public class UISettingPair
{
    string name;
    public Button btn;
    public InputField input;
    public FieldInfo field;
    public Settings settings;

    public UISettingPair(string name, Button btn, InputField input, FieldInfo field, Settings settings)
    {
        this.name = name;
        this.btn = btn;
        this.input = input;
        this.field = field;
        this.settings = settings;

        addListener();
        refreshValue();
    }

    public void refreshValue()
    {
        input.text = field.GetValue(settings).ToString();
    }

    public void addListener()
    {
        btn.onClick.AddListener(listener);
    }

    public void listener()
    {
        object temp = field.GetValue(settings);
        if ( temp is float )
        {
            float value = float.Parse(input.text);
            field.SetValue(settings, value);
        } else if ( temp is int)
        {
            int value = int.Parse(input.text);
            field.SetValue(settings, value);
        } else
        {
            Debug.LogError("Unable to find type");
        }
    }
}


public class Settings
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


    // Note implemented
    public float FrictionRoad = 0.8f;
    public float FrictionGround = 0.4f;
}