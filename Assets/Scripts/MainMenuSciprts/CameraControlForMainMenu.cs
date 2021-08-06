using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControlForMainMenu : MonoBehaviour
{
    public Camera[] cams;

    int currentCam = 0;
    int previousCam = 1;

    private void Start()
    {
        foreach(Camera cam in cams)
        {
            cam.gameObject.SetActive(false);
        }
        cams[0].gameObject.SetActive(true);
        InvokeRepeating("changeCamera", 5f, 10f);
    }

    void changeCamera()
    {
        previousCam = currentCam;
        currentCam = currentCam == 0 ? 1 : 0;
        cams[currentCam].gameObject.SetActive(true);
        cams[previousCam].gameObject.SetActive(false);
    }


    // Update is called once per frame
    void Update()
    {
       cams[currentCam].transform.RotateAround(cams[currentCam].transform.position, new Vector3(0, 1, 0), 0.05f);
    }
}
