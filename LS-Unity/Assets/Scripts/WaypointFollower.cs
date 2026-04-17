using UnityEngine;

public class WaypointFollower : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 25f;
    public float rotationSpeed = 5f;

    [Header("Behavior Options")]
    public bool returnToStart = true;     // Option 1: Return after last waypoint
    public bool turnAtEnd = false;        // Option 2: Turn 180° after last waypoint

    private int currentWaypointIndex = 0;
    private Animator animator;

    private bool isReturning = false;
    private bool isTurning = false;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Quaternion targetTurnRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    void Update()
    {
        if (waypoints.Length == 0 || !animator.GetBool("isWalking")) return;

        if (isTurning)
        {
            Perform180Turn();
            return;
        }

        if (isReturning)
        {
            ReturnToStart();
            return;
        }

        Transform target = waypoints[currentWaypointIndex];
        Vector3 direction = (target.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= waypoints.Length)
            {
                // Only do one of the two behaviors
                if (turnAtEnd)
                {
                    Start180Turn();
                }
                else if (returnToStart)
                {
                    isReturning = true;
                }
                else
                {
                    StopWalking();
                }
            }
        }
    }

    private void Start180Turn()
    {
        isTurning = true;
        targetTurnRotation = Quaternion.Euler(0, transform.eulerAngles.y + 180f, 0);
    }

    private void Perform180Turn()
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetTurnRotation, rotationSpeed * 100f * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetTurnRotation) < 1f)
        {
            transform.rotation = targetTurnRotation;
            isTurning = false;
            StopWalking();
        }
    }

    private void ReturnToStart()
    {
        Vector3 direction = (startPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (Vector3.Distance(transform.position, startPosition) < 0.1f)
        {
            transform.rotation = startRotation;
            isReturning = false;
            StopWalking();
        }
    }

    public void StartWalking()
    {
        currentWaypointIndex = 0;
        animator.SetBool("isWalking", true);
        enabled = true;
    }

    public void StopWalking()
    {
        animator.SetBool("isWalking", false);
        enabled = false;
    }
}
