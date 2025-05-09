using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 1.5f;
    public float acceleration = 15f;
    public float deceleration = 10f;
    public float reverseSpeed = 1f;

    [Header("Steering Settings")]
    public float turnSpeed = 200f;
    public float turnSpeedAtMaxSpeed = 150f;
    public float driftFactor = 0.95f;

    [Header("Wheel Visuals")]
    public Transform[] frontWheels;
    public Transform[] rearWheels;
    public float wheelSpinSpeedMultiplier = 300f;
    public float maxSteeringAngle = 25f;

    [Header("Tire Smoke Effects")]
    public ParticleSystem[] rearTireSmoke;
    public float minSpeedForSmoke = 0.8f;
    public float minSteerForSmoke = 15f;
    public float maxEmissionRate = 25f;
    [Range(0, 1)] public float smokeIntensity = 0.5f;

    private FloatingJoystick joystick;
    private Rigidbody rb;
    private float currentSpeed;
    private bool isReversing;
    private float currentSteerAngle;
    private float[] baseEmissionRates;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = new Vector3(0, -0.5f, 0);

        // Initialize smoke systems
        baseEmissionRates = new float[rearTireSmoke.Length];
        for (int i = 0; i < rearTireSmoke.Length; i++)
        {
            if (rearTireSmoke[i] != null)
            {
                baseEmissionRates[i] = rearTireSmoke[i].emission.rateOverTime.constant;
                rearTireSmoke[i].Stop();
            }
        }
    }

    private void Start()
    {
        joystick = Object.FindFirstObjectByType<FloatingJoystick>();
    }

    private void FixedUpdate()
    {
        float verticalInput = joystick.Vertical;
        float horizontalInput = joystick.Horizontal;

        HandleAcceleration(verticalInput);
        HandleSteering(horizontalInput);
        ApplyMovement();
    }

    private void Update()
    {
        UpdateWheelVisuals();
        UpdateTireSmoke();
    }

    private void HandleAcceleration(float verticalInput)
    {
        if (verticalInput > 0.1f)
        {
            isReversing = false;
            currentSpeed = Mathf.Lerp(currentSpeed, maxForwardSpeed * verticalInput, acceleration * Time.fixedDeltaTime);
        }
        else if (verticalInput < -0.1f)
        {
            isReversing = true;
            currentSpeed = Mathf.Lerp(currentSpeed, reverseSpeed * verticalInput, acceleration * 0.7f * Time.fixedDeltaTime);
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0, deceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleSteering(float horizontalInput)
    {
        if (Mathf.Abs(currentSpeed) > 0.01f)
        {
            float speedRatio = Mathf.Abs(currentSpeed) / maxForwardSpeed;
            float currentTurnSpeed = Mathf.Lerp(turnSpeed, turnSpeedAtMaxSpeed, speedRatio);
            float turnModifier = isReversing ? -1 : 1;

            transform.Rotate(0, horizontalInput * currentTurnSpeed * Time.fixedDeltaTime * turnModifier, 0);
            currentSteerAngle = horizontalInput * maxSteeringAngle;
        }
        else
        {
            currentSteerAngle = 0;
        }
    }

    private void ApplyMovement()
    {
        Vector3 newVelocity = transform.forward * currentSpeed;
        newVelocity.y = 0;
        rb.linearVelocity = newVelocity;
    }

    private void UpdateWheelVisuals()
    {
        float spinSpeed = currentSpeed * wheelSpinSpeedMultiplier * Time.deltaTime;

        // Rear wheels only spin
        foreach (Transform wheel in rearWheels)
        {
            wheel.Rotate(spinSpeed, 0, 0);
        }

        // Front wheels spin and steer
        foreach (Transform wheel in frontWheels)
        {
            wheel.localRotation = Quaternion.Euler(
                wheel.localEulerAngles.x + spinSpeed,
                currentSteerAngle,
                wheel.localEulerAngles.z
            );
        }
    }

    private void UpdateTireSmoke()
    {
        bool shouldEmit = Mathf.Abs(currentSteerAngle) > minSteerForSmoke &&
                         Mathf.Abs(currentSpeed) > minSpeedForSmoke;

        float intensity = Mathf.Clamp01(
            Mathf.Abs(currentSteerAngle) / maxSteeringAngle * smokeIntensity * 2f
        );

        for (int i = 0; i < rearTireSmoke.Length; i++)
        {
            if (rearTireSmoke[i] == null) continue;

            var emission = rearTireSmoke[i].emission;
            emission.rateOverTime = intensity * maxEmissionRate;

            if (shouldEmit)
            {
                if (!rearTireSmoke[i].isPlaying)
                    rearTireSmoke[i].Play();
            }
            else
            {
                if (rearTireSmoke[i].isPlaying)
                    rearTireSmoke[i].Stop();
            }
        }
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Speed: {currentSpeed.ToString("F1")}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Steering: {currentSteerAngle.ToString("F1")}°");
        GUI.Label(new Rect(10, 50, 300, 20), $"Smoke: {(IsEmittingSmoke() ? "ON" : "OFF")}");
    }

    private bool IsEmittingSmoke()
    {
        return rearTireSmoke.Length > 0 && rearTireSmoke[0].isPlaying;
    }
}