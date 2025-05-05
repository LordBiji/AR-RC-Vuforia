using UnityEngine;
using Vuforia;

[RequireComponent(typeof(Rigidbody))]
public class CubeController : MonoBehaviour
{
    [Header("Joystick Reference")]
    public FloatingJoystick movementJoystick;

    [Header("Movement Settings")]
    public float moveForce = 10f;
    public float rotationTorque = 5f;
    public float groundStickForce = 20f;
    public float maxSpeed = 3f;
    public float groundOffset = 0.05f;

    private Rigidbody rb;
    private bool usePhysicsMovement = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void FixedUpdate() // Gunakan FixedUpdate untuk physics
    {
        if (usePhysicsMovement)
        {
            ApplyMovement();
            StickToGroundPhysics();
        }
    }

    void Update() // Gunakan Update untuk input dan non-physics
    {
        // Tetap bisa pakai keyboard debug
        Vector2 input = GetInput();

        if (!usePhysicsMovement)
        {
            SimpleMove(input);
        }
    }

    private Vector2 GetInput()
    {
        // Keyboard debug
        Vector2 keyboardInput = new Vector2(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical")
        );

        if (keyboardInput != Vector2.zero)
            return keyboardInput;

        // Joystick mobile
        return new Vector2(
            movementJoystick.Horizontal,
            movementJoystick.Vertical
        );
    }

    private void ApplyMovement()
    {
        Vector2 input = GetInput();

        // Gerakan maju/mundur dengan AddForce
        Vector3 moveForceVector = transform.forward * input.y * moveForce;
        rb.AddForce(moveForceVector);

        // Batasi kecepatan maksimal
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }

        // Rotasi dengan torque
        float turn = input.x * rotationTorque;
        rb.AddTorque(transform.up * turn);
    }

    private void StickToGroundPhysics()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 1.5f))
        {
            // Hitung gaya untuk menempel ke ground
            Vector3 stickForce = Vector3.up * groundStickForce;
            rb.AddForce(stickForce, ForceMode.Acceleration);

            // Adjust position slightly
            float yPos = hit.point.y + groundOffset;
            if (Mathf.Abs(transform.position.y - yPos) > 0.1f)
            {
                Vector3 newPos = new Vector3(
                    transform.position.x,
                    yPos,
                    transform.position.z
                );
                rb.MovePosition(newPos);
            }
        }
    }

    private void SimpleMove(Vector2 input)
    {
        // Alternatif non-physics movement
        Vector3 move = transform.forward * input.y * moveForce * Time.deltaTime;
        transform.position += move;

        float turn = input.x * rotationTorque * Time.deltaTime;
        transform.Rotate(0, turn, 0);
    }

    public void ToggleMovementMode()
    {
        usePhysicsMovement = !usePhysicsMovement;
        rb.isKinematic = !usePhysicsMovement;
    }
}