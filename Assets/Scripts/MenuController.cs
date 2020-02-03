using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Server server = null;

    enum Windows : int  { Menu, Credits, Rules , Lobby, Settings}
    //Canvas
    [SerializeField] private GameObject canvas_menu = null;
    [SerializeField] private GameObject canvas_credits = null;
    [SerializeField] private GameObject canvas_rules = null;
    [SerializeField] private GameObject canvas_lobby = null;
    [SerializeField] private GameObject canvas_settings = null;

    // Menu buttons
    [SerializeField] private Button menu_credits = null;
    [SerializeField] private Button menu_exit = null;
    [SerializeField] private Button menu_rules = null;
    [SerializeField] private Button menu_start = null;

    // Credits buttons
    [SerializeField] private Button credits_back = null;

    // Rules buttons
    [SerializeField] private Button rules_back = null;

    // Lobby buttons
    [SerializeField] private Button lobby_back = null;
    [SerializeField] private Button lobby_start = null;

    // Settings buttons
    [SerializeField] private Button settings_back = null;


    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        
        // Menu buttons
        menu_start.onClick.AddListener(() => SwapTo(Windows.Lobby));
        menu_credits.onClick.AddListener(() => SwapTo(Windows.Credits));
        menu_rules.onClick.AddListener(() => SwapTo(Windows.Rules));
        menu_exit.onClick.AddListener(ExitGame);

        // Credits buttons
        credits_back.onClick.AddListener(() => SwapTo(Windows.Menu));

        // Rules buttons
        rules_back.onClick.AddListener(() => SwapTo(Windows.Menu));

        // Lobby buttons
        lobby_back.onClick.AddListener(() => SwapTo(Windows.Menu));
        lobby_back.onClick.AddListener(server.StartServer);

        // Settings buttons
        settings_back.onClick.AddListener(() => SwapTo(Windows.Settings));
    }

    // Update is called once per frame
    void Update()
    {

    }


    void UpdateLobbyPlayers()
    {
        GameObject lobby_area_players = canvas_lobby.transform.GetChild(0).gameObject; // What on earth is this hack?
    }

    void SwapTo(Windows state)
    {
        canvas_menu.SetActive(false);
        canvas_rules.SetActive(false);
        canvas_lobby.SetActive(false);
        canvas_credits.SetActive(false);
        canvas_settings.SetActive(false);

        switch (state)
        {
            case Windows.Menu:
                canvas_menu.SetActive(true);
                break;
            case Windows.Credits:
                canvas_credits.SetActive(true);
                break;
            case Windows.Rules:
                canvas_rules.SetActive(true);
                break;
            case Windows.Lobby:
                canvas_lobby.SetActive(true);
                break;
            case Windows.Settings:
                canvas_settings.SetActive(true);
                break;
            default:
                Debug.LogError("Trying to set invalid state");
                break;
        }
    }

    void ExitGame()
    {
        print("Exit game");
        // save any game data here
        #if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}
