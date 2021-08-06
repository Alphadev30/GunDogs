using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;

public class RoomButtonScript : MonoBehaviour
{
    public Text btnText;
    private RoomInfo info;

    public void SetBtnDetails(RoomInfo roomInfo)
    {
        info = roomInfo;
        btnText.text = info.Name;
    }

    public void openRoom()
    {
        Launcher.instance.JoinRoom(info);
    }

}
