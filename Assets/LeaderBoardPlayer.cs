using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardPlayer : MonoBehaviour
{
    public Text playerName, killsTxt, deathsTxt;

    public void setInfo(string name, int kills, int deaths)
    {
        playerName.text = name;
        killsTxt.text = kills.ToString();
        deathsTxt.text = deaths.ToString();
    }
}
