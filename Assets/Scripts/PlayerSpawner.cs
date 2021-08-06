using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

/// <summary>
/// This script is the heart of Player spawning takes the spawning points from the SpawnManagerScript.
/// </summary>

public class PlayerSpawner : MonoBehaviour
{
    public static PlayerSpawner instance;

    private void Awake()
    {
        instance = this;
    }

    public GameObject playerPrefab;
    private GameObject player;

    public GameObject deathEfect;

    int resTimer = 4;

    // Start is called before the first frame update
    void Start()
    {
        if(PhotonNetwork.IsConnected)
        {
            SpawnPlayer();
        }
    }

    public void SpawnPlayer()
    {
        Transform spawmPoint = SpawnManager.instance.GetSpawnPoints();
        player = PhotonNetwork.Instantiate(playerPrefab.name, spawmPoint.position, spawmPoint.rotation);
        
    }

    public void Die(string hitBy)
    {
        UIController.instance.killedByTxt.text = "You were killed by " + hitBy;
        resTimer = 4;

        MatchManager.instance.updateStatsSend(PhotonNetwork.LocalPlayer.ActorNumber, 1, 1);

        if (player != null)
        {
            StartCoroutine(killCou());
        }
    }

    //Spawns the player after 4 seconds of this death and start the 4 second timer.
    IEnumerator killCou()
    {
        PhotonNetwork.Instantiate(deathEfect.name, player.transform.position, Quaternion.identity);
        PhotonNetwork.Destroy(player);
        UIController.instance.DeathScreen.SetActive(true);

        StartCoroutine(ResTimer());

        yield return new WaitForSeconds(4.3f);

        UIController.instance.DeathScreen.SetActive(false);
        SpawnPlayer();
    }

    IEnumerator ResTimer()
    {
        yield return new WaitForSeconds(1f);
        UIController.instance.Timer.text = "Respawning in " + resTimer.ToString();
        resTimer--;
        if (resTimer >= 0)
        {
            StartCoroutine(ResTimer());
        }
        else
        {
            StopCoroutine(ResTimer());
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
