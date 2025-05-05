using UnityEngine;
using Vuforia;

[RequireComponent(typeof(Rigidbody))]
public class RCCarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 8f;
    public float maxReverseSpeed = 4f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float turnSpeed = 120f;
    public float gravityMultiplier = 2f;

    [Header("AR Placement")]
    public float groundOffset = 0.05f;
    public bool autoPlaceOnStart = true;

    private Rigidbody rb;
    private float currentSpeed = 0f;
    private Vector2 inputDirection = Vector2.zero;
    private bool isGrounded = false;
    private bool isPlaced = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = Vector3.down * 0.2f; // Lower center of mass for better stability
    }

    void Start()
    {
        //if (autoPlaceOnStart)
        //{
        //    PlaceCarAtCenter();
        //}
    }

    void Update()
    {
        if (!isPlaced) return;

        HandleMovement();
        ApplyGroundStick();
    }

    void FixedUpdate()
    {
        if (!isPlaced) return;

        ApplyMovementForces();
    }

    private void HandleMovement()
    {
        // Acceleration/deceleration
        if (inputDirection.y > 0.1f) // Forward
        {
            currentSpeed = Mathf.Lerp(currentSpeed, maxForwardSpeed, acceleration * Time.deltaTime);
        }
        else if (inputDirection.y < -0.1f) // Reverse
        {
            currentSpeed = Mathf.Lerp(currentSpeed, -maxReverseSpeed, acceleration * Time.deltaTime);
        }
        else // Decelerate
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0, deceleration * Time.deltaTime);
        }
    }

    private void ApplyMovementForces()
    {
        // Calculate movement direction
        Vector3 moveDirection = transform.forward * currentSpeed;

        // Apply turning if moving
        if (Mathf.Abs(currentSpeed) > 0.1f)
        {
            float turnAmount = inputDirection.x * turnSpeed * Time.fixedDeltaTime;
            transform.Rotate(0, turnAmount * Mathf.Sign(currentSpeed), 0);
        }

        // Apply movement force
        rb.AddForce(moveDirection - rb.linearVelocity, ForceMode.VelocityChange);
    }

    private void ApplyGroundStick()
    {
        // Raycast to stick to ground
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 1.5f))
        {
            isGrounded = true;
            transform.position = hit.point + Vector3.up * groundOffset;

            // Align with ground normal
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation,
                10f * Time.deltaTime
            );
        }
        else
        {
            isGrounded = false;
            // Apply extra gravity when airborne
            rb.AddForce(Physics.gravity * gravityMultiplier, ForceMode.Acceleration);
        }
    }

    /*public void PlaceCarAtCenter()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
        Ray ray = Camera.main.ScreenPointToRay(screenCenter);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 10f))
        {
            transform.position = hit.point + Vector3.up * groundOffset;

            // Face camera but keep Y rotation only
            Vector3 lookDir = Camera.main.transform.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.LookRotation(lookDir);

            isPlaced = true;
            Debug.Log("Car placed automatically at center");
        }
        else
        {
            Debug.LogWarning("No ground detected for auto placement");
            Invoke("PlaceCarAtCenter", 0.5f); // Retry after delay
        }
    }*/

    // For mobile input (call this from your UI buttons/joystick)
    public void SetInputDirection(Vector2 direction)
    {
        inputDirection = direction;
    }

    // For resetting car position
    public void ResetCarPosition()
    {
        //PlaceCarAtCenter();
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}