using System;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class Sensor : MonoBehaviour
{
    [Header("General")]
    [SerializeField] string targetTag = "PlayerBody";
    [SerializeField] float timerInterval = 0.5f;

    [Header("Vision Cone Settings")]
    public bool useVisionCone = true;
    public float sightDistance = 15f;
    public float fieldOfView = 60f;
    public float eyeHeightOffset = 1.5f;

    [Header("Trigger Radius Settings")]
    public bool useTriggerDetection = true;
    public float triggerRadius = 5f;

    public event Action OnTargetChanged = delegate { };

    public Vector3 TargetPosition => target ? target.transform.position : Vector3.zero;
    public bool IsTargetInRange => target != null;

    GameObject target;
    Vector3 lastKnownPosition;
    CountdownTimer timer;
    private PlayerController playerController;
    SphereCollider detectionRange;

    void Awake()
    {
        detectionRange = GetComponent<SphereCollider>();
        detectionRange.isTrigger = true;
        detectionRange.radius = triggerRadius;
        //playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
    }

    void Start()
    {
        timer = new CountdownTimer(timerInterval);
        timer.OnTimerStop += () => {
            ScanForTarget();
            timer.Start();
        };
        timer.Start();
    }

    void Update()
    {
        timer.Tick(Time.deltaTime);
    }

    void ScanForTarget()
    {
        GameObject found = null;

        if (useVisionCone)
        {
            GameObject player = GameObject.FindGameObjectWithTag(targetTag);
            if (player != null && IsInSight(player))
            {
                found = player;
            }
        }

        if (!useVisionCone && useTriggerDetection)
        {
            // Keep current trigger-detected target (handled by OnTriggerEnter/Exit)
            found = target;
        }

        UpdateTargetPosition(found);
    }

    bool IsInSight(GameObject candidate)
    {
        Vector3 directionToTarget = candidate.transform.position - (transform.position + Vector3.up * eyeHeightOffset);
        float angle = Vector3.Angle(directionToTarget, transform.forward);

        if (angle < fieldOfView * 0.5f && directionToTarget.magnitude < sightDistance)
        {
            Ray ray = new Ray(transform.position + Vector3.up * eyeHeightOffset, directionToTarget.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, sightDistance))
            {
                if (hit.collider.gameObject == candidate)
                {
                    Debug.DrawRay(ray.origin, ray.direction * sightDistance, Color.red);
                    playerController.Spottedtxt.SetActive(true);
                    return true;
                }
            }
        }
        playerController.Spottedtxt.SetActive(false);
        return false;
    }

    void UpdateTargetPosition(GameObject newTarget)
    {
        if (newTarget == target) return;

        target = newTarget;
        lastKnownPosition = TargetPosition;
        OnTargetChanged.Invoke();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!useTriggerDetection || !other.CompareTag("PlayerBody")) return;
        UpdateTargetPosition(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (!useTriggerDetection || !other.CompareTag("PlayerBody")) return;
        UpdateTargetPosition(null);
    }

    void OnDrawGizmosSelected()
    {
        if (useTriggerDetection)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, triggerRadius);
        }

        if (useVisionCone)
        {
            Gizmos.color = Color.yellow;
            Vector3 eyePosition = transform.position + Vector3.up * eyeHeightOffset;
            Vector3 forward = transform.forward * sightDistance;

            Quaternion leftRay = Quaternion.Euler(0, -fieldOfView / 2, 0);
            Quaternion rightRay = Quaternion.Euler(0, fieldOfView / 2, 0);

            Gizmos.DrawRay(eyePosition, leftRay * forward);
            Gizmos.DrawRay(eyePosition, rightRay * forward);
        }
    }
}
