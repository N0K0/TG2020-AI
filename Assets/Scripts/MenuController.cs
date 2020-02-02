using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    //Canvas
    [SerializeField] private Canvas canvas_menu;
    [SerializeField] private Canvas canvas_credits;

    // Menu buttons
    [SerializeField] private Button menu_credits;
    [SerializeField] private Button menu_exit;
    [SerializeField] private Button menu_rules;
    [SerializeField] private Button menu_start;

    // Credits buttons
    [SerializeField] private Button credits_back;

    // Rules buttons
    [SerializeField] private Button rules_back;



    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ExitGame()
    {
        Application.Quit();
    }

    private void OnGUI()
    {
        
    }
}
