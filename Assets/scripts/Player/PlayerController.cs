using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class PlayerController : MonoBehaviour
{
    [Header("system")]
    float horizontalInput;
    float verticalInput;
    public float gravity;
    public GameObject Sbuttons;
    public GameObject Lbuttons;
    public GameObject Rbuttons;
    public GameObject Spottedtxt;
    public movmentstate state;
    Vector3 moveDirection;
    Rigidbody rb;
    public LayerMask Ground;
    public CloudSave CloudSave;
    private string CombinedFilePath;
    public string FileName = "jump.wav";
    public string FolderName = "audio";

    public AudioSource audioSource;
    private AudioClip clip;

    [Header("player")]
    public Transform orientation;

    [Header("movment")]
    private float moveSpeed;
    public float walkingspeed;
    public float sprintspeed;
    public float GDrag;

    [Header("jumping")]
    public float playerhight;
    public float jumpF;
    public float jumpC;
    public float airM;

    [Header("crouching")]
    public float crouchspeed;
    public float crouchYScale;
    float startYScale;

    [Header("bool")]
    [SerializeField]private bool grounded;
    [SerializeField]private bool canJump;
    [SerializeField]private bool crouthing;
    [SerializeField]private bool exitingslop;
    [Header("keybinds")]
    public KeyCode jumpkey = KeyCode.Space;
    public KeyCode sprintkey = KeyCode.LeftShift;
    public KeyCode crouchkey = KeyCode.LeftControl;

    [Header("slope")]
    public float maxSlopeangl;
    private RaycastHit slopeHit;
    
    public enum movmentstate
    {
        walking,
        sprinting,
        air,
        crouching
    }
    void Start()
    {
        CombinedFilePath = Path.Combine(Application.streamingAssetsPath,FolderName,FileName);

    startYScale = transform.localScale.y;

        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        canJump = true;
        Spottedtxt.SetActive(false);

        if (audioSource == null)
        {
            Debug.Log("erm no audio source?");
            return;
        }
        loadsoundfromfile();
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerhight * 0.5f + 0.2f, Ground);
        MyInputs();
        speedCom();
        states();
     
        

        if (grounded)
            rb.drag = GDrag;
        else
            rb.drag = 0;

    }

    private void FixedUpdate()
    {
        moveplayer();
        Gravity();
    }

    void MyInputs()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Sbuttons.SetActive(true);
            Lbuttons.SetActive(true);
            Rbuttons.SetActive(true);
            Time.timeScale = 0f;
        }
        else if (Input.GetKeyUp(KeyCode.Tab))
        {
            Sbuttons.SetActive(false);
            Lbuttons.SetActive(false);
            Rbuttons.SetActive(false);
            Time.timeScale = 1f;
        }

        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(jumpkey) && canJump && grounded)
        {
            canJump = false;

            jump();
            Invoke(nameof(resetjump), jumpC);
        }

        if (Input.GetKeyDown(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 10f, ForceMode.Impulse);
            crouthing = true;
        }

        if (Input.GetKeyUp(crouchkey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            crouthing = false;
        }
    }
    void moveplayer()
    {
        if (OnSlope() && !exitingslop)
        {
            rb.AddForce(getslopmovedirection() * moveSpeed * 20f, ForceMode.Force);
            
            if(rb.velocity.y > 0)
            {
                rb.AddForce(Vector3.down * 80f,ForceMode.Force );
            }
        }
            
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airM, ForceMode.Force);
        }
    }

    void speedCom()
    {
        if (OnSlope() && !exitingslop) 
        {
            if (rb.velocity.magnitude > moveSpeed)
            {
            rb.velocity = rb.velocity.normalized * moveSpeed;
            }   
        }
        
        else 
        { 
            Vector3 FlatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            if (FlatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = FlatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
            
        }
    }

    void jump()
    {
        //CloudSave.addjump();
        //playsound();
        exitingslop = true;

        if (crouthing)
    {
        return;
    }
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpF, ForceMode.Impulse);
    }

    void resetjump()
    {
        canJump = true;
        exitingslop = false;
    }

    void states()
    {
        if (Input.GetKeyDown(crouchkey)) 
        { 
            state = movmentstate.crouching;
            moveSpeed = crouchspeed;
        }
        if (grounded && Input.GetKey(sprintkey) && !crouthing)
        {
            state = movmentstate.sprinting;
            moveSpeed = sprintspeed;
        }
        else if (grounded && !crouthing)
        {
            state = movmentstate.walking;
            moveSpeed = walkingspeed;
        }
        else if (!crouthing)
        {
            state = movmentstate.air;
        }

    }
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerhight + 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up,slopeHit.normal); 
        }
        return false;
    }

    private Vector3 getslopmovedirection()
    { 
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal); 
    }

    void Gravity()
    {
        rb.AddForce(Vector3.down * gravity, ForceMode.Impulse);
    }

    void loadsoundfromfile()
    {
        if (File.Exists(CombinedFilePath)) 
        {
            byte[] audio = File.ReadAllBytes(CombinedFilePath);

            float[] FloatArray = new float[audio.Length /2];

            for (int i = 0; i < FloatArray.Length; i++)
            {
                short bitvalue = System.BitConverter.ToInt16(audio, i * 2);

                FloatArray[i] = bitvalue / 3768.0f;
            }

            clip = AudioClip.Create("Jump", FloatArray.Length, 1, 44800, false);

            clip.SetData(FloatArray, 0);
        }
        else
        {
            Debug.Log("lier lier files not thier");
        }
    }

    void playsound()
    {
        if (clip == null || audioSource == null)
        {
            return;
        }
        audioSource.PlayOneShot(clip);
    }
}