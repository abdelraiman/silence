using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [SerializeField] float DetectionRadius = 5f;
    [SerializeField] float TimerInterval = 1f;

    SphereCollider DetectionRange;

    public event Action OnTargetChanged = delegate { };
    public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
    public bool IsTargetInRange => TargetPosition != Vector3.zero;


    GameObject target;
    Vector3 LastKnownPosition;
    CountDownTimer timer;

    void Awake()
    {
        DetectionRange = GetComponent<SphereCollider>();
        DetectionRange.isTrigger = true;
        DetectionRange.radius = DetectionRadius;
    }

    void Start()
    {
        timer = new CountDownTimer(TimerInterval);
        timer.OnTimerStop += () =>
        {
            UpdateTargetPosition(target.OrNull());
            timer.Start();
        };
        timer.Start();
    }

    void Update()
    {
        timer.Tick(Time.deltaTime);
    }
    void UpdateTargetPosition(GameObject target = null)
    {
        this.target = target;
        if (IsTargetInRange && (LastKnownPosition != TargetPosition || LastKnownPosition != Vector3.zero))
        {
            LastKnownPosition = TargetPosition;
            OnTargetChanged.Invoke();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("player")) return;
        UpdateTargetPosition(other.gameObject);
    }
    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("player")) return;
        UpdateTargetPosition();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = IsTargetInRange ? Color.red : Color.green;
        Gizmos.DrawWireSphere(TargetPosition, DetectionRadius);
    }
}