using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class extrasForGame : MonoBehaviour
{
    // InputName
    public InputField playerGameName;

    public void playerGameNamechanger()
    {
        if(playerGameName.text != "")
        {
            PlayerPrefs.SetString("PlayerName", playerGameName.text.ToString());
            SceneManager.LoadScene(0);

        }
        else
        {
            Debug.Log("Player name cannot be empty");
        }
    }
}
