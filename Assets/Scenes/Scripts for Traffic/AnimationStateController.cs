using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationStateController : MonoBehaviour
{
    private Animator animator;
    public Vector3 destination;
    private Vector3 previousPosition;
    public bool reachDestination;
    public string collisionTag;

    [SerializeField] private float stopDistance = 0.1f;
    [SerializeField] private float rotationSpeed = 360f;

    [SerializeField] private float normalSpeedMin = 1.0f;
    [SerializeField] private float normalSpeedMax = 2.5f;

    [SerializeField] private float collisionRotationAngle = 30f; // Angle to rotate on collision

    [SerializeField] private float decelerationDuration = 1.0f;
    [SerializeField] private float accelerationDuration = 1.0f;

    private static readonly int ForwardParam = Animator.StringToHash("Forward");
    private static readonly int HorizontalParam = Animator.StringToHash("Horizontal");


    private float movementSpeed;
    private float originalSpeed;

    private float currentSpeed = 0f;
    private bool isDecelerating = false;


    // audio for footstep
    public float footstepInterval = 0.5f; // time between steps at constant speed (a human walks ~1.5 steps per second → stepIntervalAtConstantSpeed = 0.66f_
    private AudioSource audioSource;
    private float timer = 0f;

    // private Quaternion originalRotation;
    private bool isRecoveringFromCollision = false;
    private Vector3 lastMovementDirection = Vector3.forward; // default init


    // detect cars
    private Coroutine decelerationCoroutine;


    void Awake()
    {
        movementSpeed = Random.Range(normalSpeedMin, normalSpeedMax);
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        previousPosition = transform.position;
        reachDestination = false;
        // destination = transform.position; // Initialize destination

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1.0f; // 3D sound
    }

    void Update()
    {
        DetectCarAhead();
        MoveTowardsDestination();
        UpdateAnimatorParameters();
        
    }

    private void FixedUpdate()
    {
        PlayFootstepSound();
    }

    void PlayFootstepSound()
    {
        if (movementSpeed > 0.01f)
        {
            timer += Time.fixedDeltaTime;

            if (timer >= footstepInterval && !audioSource.isPlaying)
            {
                audioSource.Play();
                timer = 0f;
            }
        }
        else
        {
            timer = 0f; // reset timer if idle
        }
    }

    void DetectCarAhead()
    {
        Vector3 rayOrigin = transform.position;// + Vector3.up * 1f;
        Vector3 forward = transform.forward;
        float detectionRange = 5f;

        if (Physics.Raycast(rayOrigin, forward, out RaycastHit hit, detectionRange))
        {
            if (hit.collider.CompareTag("Car"))
            {
                // Debug.DrawRay(rayOrigin, forward * detectionRange, Color.blue);
                Debug.Log($"{gameObject.name} sees car {hit.collider.name}, stopping.");

                if (!isDecelerating)
                {
                    if (decelerationCoroutine != null)
                        StopCoroutine(decelerationCoroutine);

                    decelerationCoroutine = StartCoroutine(DecelerateTemporarily());
                }
            }
        }
    }
    IEnumerator DecelerateTemporarily()
    {
        isDecelerating = true;
        movementSpeed = 0f;
        var currentSpeedTemp = currentSpeed;

        float holdTime = 2f;
        yield return new WaitForSeconds(holdTime);

        movementSpeed = currentSpeedTemp;
        isDecelerating = false;
    }

    void MoveTowardsDestination()
    {

        if (transform.position != destination)
        {

            Vector3 destinationDirection = destination - transform.position;
            destinationDirection.y = 0;
            float distance = destinationDirection.magnitude;

            if (distance > stopDistance)
            {
                reachDestination = false;

                // Update rotation only if not recovering
                if (!isRecoveringFromCollision)
                {
                    if (destinationDirection.sqrMagnitude > 0.001f)
                    {
                        lastMovementDirection = destinationDirection.normalized;
                        Quaternion targetRotation = Quaternion.LookRotation(lastMovementDirection);

                        // Smooth rotation toward the target
                        // transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                    }
                }

                // Smooth acceleration/deceleration
                float speedTarget = movementSpeed;
                float lerpTime = isDecelerating ? decelerationDuration : accelerationDuration;
                currentSpeed = Mathf.Lerp(currentSpeed, speedTarget, Time.deltaTime / Mathf.Max(lerpTime, 0.01f));

                // Move along last known direction if recovering
                Vector3 movementDirection = isRecoveringFromCollision ? lastMovementDirection : destinationDirection.normalized;
                transform.position += movementDirection * currentSpeed * Time.deltaTime;
            }
            else
            {
                reachDestination = true;
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag(collisionTag) || other.gameObject.CompareTag("MainCamera"))
        {
            Debug.Log($"{gameObject.name} collided with {other.gameObject.name}");


            // Get direction to the other object
            Vector3 directionToOther = (other.transform.position - transform.position).normalized;
            // Get forward direction 
            Vector3 forwardDir = transform.forward;

           

            // Compute signed angles
            float angle = Vector3.Angle(forwardDir, directionToOther); // 
            float signedAngle = Vector3.SignedAngle(forwardDir, directionToOther, Vector3.up); // >0 = left, <0 = right

            if (angle < 60f) // Only react if object is roughly in front
            {
                Debug.DrawRay(transform.position, forwardDir * 5, Color.green, 1f);
                Debug.DrawRay(transform.position, directionToOther * 5, Color.yellow, 1f);

                // Decide rotation direction: turn away from incoming object
                float rotationDirection = signedAngle < 0f ? 1f : -1f;

                // Apply rotation
                // float rotationAmount = collisionRotationAngle * rotationDirection;
                // cosine-based fall off  (Human-Like): Cosine is 1 when angle = 0 (head-on); Cosine is 0 when angle = 90(side)
                // the smaller the angle, the more rotation needed for collision avoidance
                float weight = Mathf.Clamp01(Mathf.Cos(angle * Mathf.Deg2Rad));  // range: 0 (90°) to 1 (0°)
                float rotationAmount_min = 10f;
                float rotationAmount_max = 20f;
                float rotationAmount = Mathf.Lerp(rotationAmount_min, rotationAmount_max, weight) * rotationDirection;
                //transform.Rotate(0, rotationAmount, 0);
                // Compute the desired rotation offset
                Quaternion rotationOffset = Quaternion.Euler(0, rotationAmount, 0);
                // Apply smooth rotation relative to current rotation
                Quaternion targetRotation = transform.rotation * rotationOffset;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                float waitTime = Mathf.Clamp(weight, 0.2f, 1.0f);
                StartCoroutine(RestoreRotationAfterSeconds(waitTime));

            }


            /* float rotationDirection = Random.value < 0.5f ? -1f : 1f;
             transform.Rotate(0, collisionRotationAngle * rotationDirection, 0);*/
        }


    }

    IEnumerator RestoreRotationAfterSeconds(float seconds)
    {
        // yield return new WaitForSeconds(seconds);

        isRecoveringFromCollision = true;

        yield return new WaitForSeconds(seconds);

        isRecoveringFromCollision = false;
    }




    void UpdateAnimatorParameters()
    {
        Vector3 movement = transform.position - previousPosition;
        previousPosition = transform.position;

        var velocityMagnitude = movement.magnitude;

        var currentStatus = animator.GetFloat("walkingFloat");

        if (velocityMagnitude < normalSpeedMin / Time.deltaTime) // i.e., normalSpeedMin/Time.DeltaTime
        {
            if (currentStatus > 0f)
                animator.SetFloat("walkingFloat", -1f);    
        }
        else
        {
             animator.SetFloat("walkingFloat", velocityMagnitude);
        }


/*        if (reachDestination)
        {
            animator.SetFloat("walkingFloat", 0);
        }*/

    }

    public void SetDestination(Vector3 newDestination)
    {
        destination = newDestination;
        reachDestination = false;
    }

    public float getcurrentSpeed()
    {
        return currentSpeed;
    }
}
