using UnityEngine;
using Vuforia;

public class TestControll : MonoBehaviour
{
    // AR Placement
    public AnchorBehaviour groundAnchor;
    private bool isPlaced = false;

    // Touch Controls
    public PrometeoTouchInput throttleButton;
    public PrometeoTouchInput brakeButton;
    public PrometeoTouchInput leftButton;
    public PrometeoTouchInput rightButton;

    // Movement Settings
    public float moveSpeed = 2f;
    public float turnSpeed = 60f;
    public float reverseSpeed = 1f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // AR Placement setup
        if (groundAnchor != null)
        {
            groundAnchor.OnTargetStatusChanged += OnAnchorStatusChanged;
        }
    }

    void OnAnchorStatusChanged(ObserverBehaviour behaviour, TargetStatus status)
    {
        if (status.Status == Status.TRACKED || status.Status == Status.EXTENDED_TRACKED)
        {
            isPlaced = true;
            rb.isKinematic = false;
            Debug.Log("Car placed on ground!");
        }
        else
        {
            isPlaced = false;
            rb.isKinematic = true;
        }
    }

    void Update()
    {
        if (!isPlaced) return;

        // Movement
        if (throttleButton != null && throttleButton.buttonPressed)
        {
            MoveForward();
        }
        else if (brakeButton != null && brakeButton.buttonPressed)
        {
            MoveBackward();
        }

        // Turning
        if (leftButton != null && leftButton.buttonPressed)
        {
            TurnLeft();
        }
        else if (rightButton != null && rightButton.buttonPressed)
        {
            TurnRight();
        }
    }

    void MoveForward()
    {
        rb.AddForce(transform.forward * moveSpeed, ForceMode.Force);
    }

    void MoveBackward()
    {
        rb.AddForce(-transform.forward * reverseSpeed, ForceMode.Force);
    }

    void TurnLeft()
    {
        transform.Rotate(0, -turnSpeed * Time.deltaTime, 0);
    }

    void TurnRight()
    {
        transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
    }

    void OnDestroy()
    {
        if (groundAnchor != null)
        {
            groundAnchor.OnTargetStatusChanged -= OnAnchorStatusChanged;
        }
    }
}