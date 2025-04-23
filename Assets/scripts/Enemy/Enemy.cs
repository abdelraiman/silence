using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.PackageManager;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] private string curentstate;
    [SerializeField] private GameObject player;
    private StateMachine stateMachine;
    private NavMeshAgent agent;
    private Vector3 LastKnown;

    public float Attackdistance = 4;
    public NavMeshAgent Agent { get => agent; }
    public GameObject Player { get => player; }
    public Vector3 lastKnown { get => LastKnown; set => LastKnown = value; }
    public path paath;

    [Header("sight values")]
    public float sightdistance = 20f;
    public float fov = 60;
    public float eyehight;
    public bool sawplayer;

    [Header("audio")]
    public string FileName = "ceeday-huh-sound-effect.mp3";
    public string FolderName = "audio";

    public AudioSource audioSource;
    private AudioClip clip;
    private string CombinedFilePath;


    void Start()
    {
        CombinedFilePath = Path.Combine(Application.streamingAssetsPath, FolderName, FileName);
        stateMachine = GetComponent<StateMachine>();
        agent = GetComponent<NavMeshAgent>();
        stateMachine.Initialise();
        player = GameObject.FindGameObjectWithTag("Player");
        loadsoundfromfile();
    }

    void Update()
    {
        canseeplayer();
        curentstate = stateMachine.activeState.ToString();
    }

    public bool InAttackRange()
    {
        return Vector3.Distance(transform.position, player.transform.position) <= Attackdistance;
    }
    public bool canseeplayer()
    {
        if (player != null)
        {
            //Debug.Log("player is there");
            if (Vector3.Distance(transform.position,player.transform.position) < sightdistance)
            {
                //Debug.Log("i think I see the player");
                Vector3 targetDirection = player.transform.position - transform.position - (Vector3.up * eyehight);
                float angelToplayer = Vector3.Angle(targetDirection, transform.forward);
                if (angelToplayer >= -fov && angelToplayer <= fov)
                {
                    Ray ray = new Ray(transform.position + (Vector3.up * eyehight), targetDirection);
                    RaycastHit Hitinfo = new RaycastHit();
                    if (Physics.Raycast(ray,out Hitinfo, sightdistance))
                    {
                        if (Hitinfo.transform.gameObject == player)
                        {
                            Debug.DrawRay(ray.origin, ray.direction * sightdistance);
                            //Debug.Log("i have line of sight");
                            
                            return true;
                        }
                    }
                    
                }
            }
        }
        return false;
    }

    void loadsoundfromfile()
    {
        if (File.Exists(CombinedFilePath))
        {
            byte[] audio = File.ReadAllBytes(CombinedFilePath);

            float[] FloatArray = new float[audio.Length / 2];

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
    public void playsound()
    {
        if (clip == null || audioSource == null)
        {
            return;
        }
        if (sawplayer == true)
        {
            return;
        }
        else
        {
            audioSource.PlayOneShot(clip);
            sawplayer = true;
        } 
    }
}
