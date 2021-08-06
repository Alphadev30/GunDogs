using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

/// <summary>
/// This Script contains Player Movements, Jump, LookAround, and Stick Camera to the Player functinalities.
/// </summary>


public class PlayerController : MonoBehaviourPunCallbacks
{
    // Camera is attached to the viewPoint
    public Transform ViewPoint;

    // Rotation Fields
    [SerializeField]
    float mouseSensitivity = 2f;
    float verticalRotStore;
    Vector2 mouseInput;

    // Movement Fields 
    [SerializeField]
    float movementSpeed = 13.5f;
    private Vector3 moveDir;
    private Vector3 movement;

    // Helpers
    CharacterController charController;
    Camera cam;

    //Jump
    float jumpForce = 4f;

    //Shooting
    public GameObject bulletImpact;
    float shotCounter;
    public float muzzleDisplayTime = 1f;

    public GameObject playerHitImpact;

    // Weapon Overheating
    public float maxHeat = 10f,  coolRate = 4f, overHeatCoolRate = 5f;
    float heatCounter;
    bool overHeated;

    // different weapons
    public Gun[] allGuns;
    int selectedGun = 0;

    //Extras
    int playerMaxHealth = 100;
    int playerCurrenHealth;
    public Animator anim;
    public GameObject playerModel;
    public Transform GunHolder, modelGunPoint;
    public Material[] allSkins;

    public float adsSpeed = 15f;
    public AudioSource footStepsSound;
    public GameObject weaponOVerheated;

    public Text playerNameTxt;

    //Mobile 
    //public Button jumpBt;

    //public Joystick joyStick;
    //public joyBtn joyButton;

    // Start is called before the first frame update
    void Start()
    {
        playerNameTxt.text = photonView.Owner.NickName.ToString();

        UIController.instance.weaponTemperatureSlider.maxValue = maxHeat;
        charController = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        cam = Camera.main;

        // Settin the skin of the player 
        playerModel.GetComponent<Renderer>().material = allSkins[photonView.Owner.ActorNumber % allSkins.Length];

        playerCurrenHealth = playerMaxHealth;
        
        if(photonView.IsMine)
        {
            playerModel.SetActive(false);
            UIController.instance.healthSlider.value = playerCurrenHealth;
        }
        else
        {
            GunHolder.parent = modelGunPoint;
            GunHolder.localPosition = Vector3.zero;
            GunHolder.localRotation = Quaternion.identity;
        }



        // For Mobile Controls : 
        //jumpBt = GameObject.Find("ButtonJump").GetComponent<Button>();
        //joyStick = FindObjectOfType<Joystick>();
        //joyButton = FindObjectOfType<joyBtn>();
    }

    // Update is called once per frame
    void Update()
    {
        // Only control me dont control other players in the room.
        if(photonView.IsMine)
        {
            PlayerRotation();
            PlayerMovements();
            StickCameraToPlayer();
            lockAndUnlockMouse();
            //Shoot
            shootConditions();
            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
            ChangeGun();
            Zoom();


            // Handle Animations
            anim.SetFloat("speed", moveDir.magnitude);
            anim.SetBool("grounded", charController.isGrounded);

            //MobileMoves();
        }

    }

    // Look Around, up using mouse Rotation
    void PlayerRotation()
    {
        mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * mouseSensitivity;

        // Code to Look left and right
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + mouseInput.x, transform.rotation.eulerAngles.z);

        // Code to look up and down
        verticalRotStore += mouseInput.y;
        verticalRotStore = Mathf.Clamp(verticalRotStore, -60, 60);
        ViewPoint.rotation = Quaternion.Euler(-verticalRotStore, transform.rotation.eulerAngles.y, transform.rotation.eulerAngles.z);
    }


    public void MobileMoves()
    {
        //moveDir = new Vector3(joyStick.Horizontal * 100f, 0f, joyStick.Vertical * 100f);

        //float yVelocity = movement.y;
        //movement = (transform.forward * moveDir.z + transform.right * moveDir.x).normalized;

        ////jumpBt.GetComponent<Button>().onClick.RemoveAllListeners();
        //jumpBt.GetComponent<Button>().onClick.AddListener(() =>
        //{
        //    movement.y += Physics.gravity.y * Time.deltaTime * 1.5f;
        //    if (charController.isGrounded)
        //    {
        //        movement.y = jumpForce;
        //        charController.Move(movement * movementSpeed * Time.deltaTime);
        //        Debug.Log("Jump");
        //    }
        //});


        //charController.Move(movement * movementSpeed * Time.deltaTime);
        
    }

    

    // Jump and Movements with Gravity
    void PlayerMovements()
    {
        moveDir = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));

        float yVelocity = movement.y;
        movement = (transform.forward * moveDir.z + transform.right * moveDir.x).normalized;

        // Apply gravity
        movement.y = yVelocity;
        if (charController.isGrounded) { movement.y = 0f; }
        movement.y += Physics.gravity.y * Time.deltaTime * 1.5f;

        if(Input.GetButtonDown("Jump") && charController.isGrounded)
        {
            movement.y = jumpForce;
        }

        charController.Move(movement * movementSpeed * Time.deltaTime);

        //Play/Stop sound fx
        if(!footStepsSound.isPlaying)
        {
            footStepsSound.Play();
        }
        if(moveDir == Vector3.zero || !charController.isGrounded)
        {
            footStepsSound.Stop();
        }
    }

    void StickCameraToPlayer()
    {
        cam.transform.position = ViewPoint.position;
        cam.transform.rotation = ViewPoint.rotation;
    }

    void lockAndUnlockMouse()
    {
        if(Input.GetKey(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if(Cursor.lockState == CursorLockMode.None)
        {
            if(Input.GetMouseButton(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }


    /// <summary>
    /// Player can shoot only 10 at a time, after few seconds the player will be able to shoot again.
    /// </summary>
    void shootConditions()
    {
        if (!overHeated)
        {
            if (Input.GetMouseButtonDown(0))
            {
                shoot();
            }
            if (Input.GetMouseButton(0) && allGuns[selectedGun].isAutomatic)
            {
                shotCounter -= Time.deltaTime;
                if (shotCounter <= 0)
                {
                    shoot();
                }
            }
            heatCounter -= coolRate * Time.deltaTime;
        }
        else
        {
            heatCounter -= overHeatCoolRate * Time.deltaTime;
            if (heatCounter <= 0)
            {
                heatCounter = 0;
                overHeated = false;
                UIController.instance.OverHeatedSymbol.SetActive(false);
            }
        }

        if (heatCounter <= 0)
        {
            heatCounter = 0;
        }
        UIController.instance.weaponTemperatureSlider.value = heatCounter;
    }


    void shoot()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        ray.origin = cam.transform.position;

        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("We hit : " + hit.collider.gameObject.name);

            // when we hit enemy
            if(hit.collider.gameObject.tag == "Player")
            {
                PhotonNetwork.Instantiate(playerHitImpact.name, hit.point, Quaternion.identity);

                // Gets the name of the player who hit us 
                hit.collider.gameObject.GetPhotonView().RPC("DealDamage", RpcTarget.All, photonView.Owner.NickName, allGuns[selectedGun].Damage, PhotonNetwork.LocalPlayer.ActorNumber) ;
            }
            else
            {
                // Instantiate bullete mark and particle effect
                GameObject impactHitPoint = Instantiate(bulletImpact, hit.point + hit.normal * 0.002f, Quaternion.LookRotation(hit.normal, Vector3.up));

                // Destroy effect after 3 seconds.
                Destroy(impactHitPoint, 3.0f);
            }

        }
        shotCounter = allGuns[selectedGun].timeBetweenShots;

        //Weapon Overheating
        heatCounter += allGuns[selectedGun].heatPerShot;
        if(heatCounter >= maxHeat)
        {
            heatCounter = maxHeat;
            overHeated = true;
            weaponOVerheated.GetComponent<AudioSource>().Play();
            UIController.instance.OverHeatedSymbol.SetActive(true);
        }

        allGuns[selectedGun].muzzleFlash.SetActive(true);
        Invoke("DisableMuzzel", 0.07f);

        //allGuns[selectedGun].gunShotSound.Stop();
        allGuns[selectedGun].gunShotSound.GetComponent<AudioSource>().Play();
    }

    void DisableMuzzel()
    {
        allGuns[selectedGun].muzzleFlash.SetActive(false);
    }

    [PunRPC]
    void DealDamage(string hitBy, int damageGiven, int actor)
    {
        takeDamage(hitBy, damageGiven, actor);
    }

    public void takeDamage(string hitBy, int damageGiven, int actor)
    {
        if(photonView.IsMine)
        {
            //Debug.Log(photonView.Owner.NickName + " Player has been hit by : " + hitBy);
            //gameObject.SetActive(false);

            if(playerCurrenHealth > 0)
            {
                playerCurrenHealth -= damageGiven;
                if(photonView.IsMine)
                UIController.instance.healthSlider.value = playerCurrenHealth;
            }
            if(playerCurrenHealth <= 0)
            {
                PlayerSpawner.instance.Die(hitBy);
                playerCurrenHealth = playerMaxHealth;
                MatchManager.instance.updateStatsSend(actor, 0, 1);
            }  
        }
    }


    void ChangeGun()
    {
        // Change gun through mouse wheel
        if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            selectedGun++;
            if(selectedGun >= allGuns.Length)
            {
                selectedGun = 0;
            }
            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }
        else if(Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            selectedGun--;
            if(selectedGun < 0)
            {
                selectedGun = allGuns.Length - 1;
            }
            //SwitchGun();
            photonView.RPC("SetGun", RpcTarget.All, selectedGun);
        }

        // change gun through button
        // selected gun = gunNo. and then SwitchGun()
    }

    //Dispaly current gun of every player;
    [PunRPC]
    public void SetGun(int GunNo)
    {
        if(GunNo < allGuns.Length)
        {
            selectedGun = GunNo;
            SwitchGun();
        }
    }

    void SwitchGun()
    {
        foreach(Gun gun in allGuns)
        {
            gun.gameObject.SetActive(false);
        }
        allGuns[selectedGun].gameObject.SetActive(true);
    }

    // Zoom out and in
    void Zoom()
    {
        if(Input.GetMouseButton(1))
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, allGuns[selectedGun].adsZoom, adsSpeed * Time.deltaTime);
        }
        else
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60f, adsSpeed * Time.deltaTime);

        }
    }

    // UI Animations 
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "poison")
        {
            UIController.instance.healthEffect.gameObject.SetActive(true);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "poison")
        {
            UIController.instance.healthEffect.gameObject.SetActive(false);
        }
    }
}
