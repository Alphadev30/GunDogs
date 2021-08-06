using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    //Weapons
    public GameObject OverHeatedSymbol;
    public Slider weaponTemperatureSlider;

    //Death Screen
    public GameObject DeathScreen;
    public Text killedByTxt;
    public Text Timer;

    //Health
    public Slider healthSlider;
    public Image healthEffect;

    //stats
    public Text kills;
    public Text deaths;

    //LeaderBoard
    public GameObject LeaderBoard;
    public LeaderBoardPlayer lBoardPlayerDisplay;

    //EndScreen
    public GameObject endRound;

    //Extras
    public Text remTimer;

    private void Awake()
    {
        instance = this;
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
