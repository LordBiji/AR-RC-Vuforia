using UnityEngine;
using Vuforia;

[RequireComponent(typeof(Rigidbody))]
public class ARCarController : MonoBehaviour
{
    // ========== AR SETTINGS ==========
    [Header("AR Configuration")]
    public PlaneFinderBehaviour planeFinder; // Changed from GroundPlaneStage
    public ContentPositioningBehaviour contentPositioner;
    public float arScale = 0.2f;
    private bool isPlaced = false;

    // ========== TOUCH CONTROLS ==========
    [Header("Touch Controls")]
    public PrometeoTouchInput throttleButton;
    public PrometeoTouchInput reverseButton;
    public PrometeoTouchInput leftButton;
    public PrometeoTouchInput rightButton;
    public PrometeoTouchInput handbrakeButton;

    // ========== WHEEL SETTINGS ==========
    [Header("Wheel Configuration")]
    public WheelCollider[] poweredWheels;
    public WheelCollider[] steeringWheels;
    public Transform[] wheelMeshes;
    public float wheelRadius = 0.15f;
    public float suspensionDistance = 0.05f;

    // ========== CAR PHYSICS ==========
    [Header("Car Physics")]
    public float maxSpeed = 25f;
    public float acceleration = 200f;
    public float brakeForce = 150f;
    public float maxSteerAngle = 25f;
    public float centerOfMassY = -0.2f;
    public float driftStiffness = 0.5f;

    // ========== EFFECTS ==========
    [Header("Effects")]
    public ParticleSystem[] tireSmoke;
    public TrailRenderer[] tireSkid;
    public AudioSource engineSound;
    public AudioSource skidSound;
    private float initialPitch;

    // ========== PRIVATE VARIABLES ==========
    private Rigidbody rb;
    private float currentSpeed;
    private bool isDrifting;

    void Start()
    {
        // Add this for better AR physics
        Physics.defaultSolverIterations = 12;
        Physics.defaultSolverVelocityIterations = 6;

        rb = GetComponent<Rigidbody>();
        SetupARVehicle();

        if (engineSound) initialPitch = engineSound.pitch;
    }

    void SetupARVehicle()
    {
        // Scale adjustment
        transform.localScale = Vector3.one * arScale;
        rb.centerOfMass = new Vector3(0, centerOfMassY, 0);

        // Wheel collider setup
        foreach (WheelCollider wheel in poweredWheels)
        {
            wheel.radius = wheelRadius;
            wheel.suspensionDistance = suspensionDistance;
            wheel.mass = 20f;
        }

        // AR placement events
        if (planeFinder && contentPositioner)
        {
            contentPositioner.OnContentPlaced.AddListener(OnContentPlaced);
        }
        else
        {
            Debug.LogWarning("AR placement components not assigned!");
        }
    }

    void OnContentPlaced(GameObject placedObject)
    {
        if (placedObject == gameObject)
        {
            isPlaced = true;
            rb.isKinematic = false;
        }
    }

    void Update()
    {
        if (!isPlaced) return;

        HandleInput();
        UpdateEffects();
        AnimateWheels();

        currentSpeed = rb.linearVelocity.magnitude * 3.6f; // Fixed velocity warning
    }

    void HandleInput()
    {
        // Throttle control
        if (throttleButton && throttleButton.buttonPressed)
        {
            foreach (WheelCollider wheel in poweredWheels)
            {
                wheel.motorTorque = acceleration;
                wheel.brakeTorque = 0;
            }
        }
        // Reverse control
        else if (reverseButton && reverseButton.buttonPressed)
        {
            foreach (WheelCollider wheel in poweredWheels)
            {
                wheel.motorTorque = -acceleration * 0.6f;
                wheel.brakeTorque = 0;
            }
        }
        else
        {
            foreach (WheelCollider wheel in poweredWheels)
            {
                wheel.motorTorque = 0;
                wheel.brakeTorque = brakeForce * 0.2f;
            }
        }

        // Steering
        float steerInput = 0;
        if (leftButton && leftButton.buttonPressed) steerInput = -1;
        if (rightButton && rightButton.buttonPressed) steerInput = 1;

        foreach (WheelCollider wheel in steeringWheels)
        {
            wheel.steerAngle = steerInput * maxSteerAngle;
        }

        // Handbrake/drifting
        if (handbrakeButton && handbrakeButton.buttonPressed)
        {
            isDrifting = true;
            foreach (WheelCollider wheel in poweredWheels)
            {
                wheel.brakeTorque = brakeForce;
                WheelFrictionCurve friction = wheel.sidewaysFriction;
                friction.stiffness = driftStiffness;
                wheel.sidewaysFriction = friction;
            }
        }
        else if (isDrifting)
        {
            isDrifting = false;
            foreach (WheelCollider wheel in poweredWheels)
            {
                WheelFrictionCurve friction = wheel.sidewaysFriction;
                friction.stiffness = 1f;
                wheel.sidewaysFriction = friction;
            }
        }
    }

    void AnimateWheels()
    {
        for (int i = 0; i < wheelMeshes.Length; i++)
        {
            if (i < poweredWheels.Length)
            {
                poweredWheels[i].GetWorldPose(out Vector3 pos, out Quaternion rot);
                wheelMeshes[i].position = pos;
                wheelMeshes[i].rotation = rot;
            }
        }
    }

    void UpdateEffects()
    {
        bool shouldEmitEffects = isDrifting || (Mathf.Abs(rb.angularVelocity.y) > 0.5f && currentSpeed > 5f);

        // Tire smoke
        if (tireSmoke != null)
        {
            foreach (ParticleSystem smoke in tireSmoke)
            {
                if (shouldEmitEffects && !smoke.isPlaying) smoke.Play();
                else if (!shouldEmitEffects && smoke.isPlaying) smoke.Stop();
            }
        }

        // Skid marks
        if (tireSkid != null)
        {
            foreach (TrailRenderer skid in tireSkid)
            {
                skid.emitting = shouldEmitEffects;
            }
        }

        // Skid sound
        if (skidSound)
        {
            if (shouldEmitEffects && !skidSound.isPlaying) skidSound.Play();
            else if (!shouldEmitEffects && skidSound.isPlaying) skidSound.Stop();
        }

        // Engine sound
        if (engineSound)
        {
            engineSound.pitch = initialPitch + (currentSpeed / maxSpeed);
        }
    }

    void FixedUpdate()
    {
        if (!isPlaced) return;

        // Speed limiter
        if (currentSpeed > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * (maxSpeed / 3.6f);
        }
    }
}