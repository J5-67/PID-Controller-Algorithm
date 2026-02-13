using UnityEngine;

public class HoverboardController : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private float hoverForce = 4f; 
    
    [Header("PID Tuning")]
    [SerializeField, Range(0, 50)] private float kp = 2f; 
    [SerializeField, Range(0, 10)] private float ki = 0f;
    [SerializeField, Range(0, 100)] private float kd = 10f; 

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 2f;

    [Header("Stability")]
    [SerializeField] private Vector3 centerOfMassOffset = new Vector3(0, -1.0f, 0); 
    [SerializeField] private float tiltAngleLimit = 30f; 
    [SerializeField] private float airResistance = 2f; 

    [SerializeField] private Transform[] thrusters;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody _rb;
    private PID[] _pidControllers;
    private float _moveInput;
    private float _turnInput;
    private Vector3 _averageGroundNormal = Vector3.up;

    private void Awake()
    {
        if (!TryGetComponent(out _rb))
        {
            return;
        }

        _rb.centerOfMass = centerOfMassOffset;
        _rb.angularDamping = 5f; 

        _pidControllers = new PID[thrusters.Length];
        for (int i = 0; i < thrusters.Length; i++)
        {
            _pidControllers[i] = new PID();
        }
    }

    private void Update()
    {
        _moveInput = Input.GetAxis("Vertical");
        _turnInput = Input.GetAxis("Horizontal");
    }

    private void FixedUpdate()
    {
        HandleHover();
        HandleMovement();
        Stabilize(); 
        ApplyAirResistance(); 
    }

    private void HandleHover()
    {
        Vector3 normalSum = Vector3.zero;
        int hitCount = 0;

        for (int i = 0; i < thrusters.Length; i++)
        {
            Transform thruster = thrusters[i];
            
            if (Physics.Raycast(thruster.position, -transform.up, out RaycastHit hit, hoverHeight * 1.5f, groundLayer))
            {
                float currentHeight = hit.distance;
                float error = hoverHeight - currentHeight;

                float output = _pidControllers[i].GetOutput(error, Time.fixedDeltaTime, kp, ki, kd);
                float clampedOutput = Mathf.Max(0f, output);

                Vector3 force = Vector3.up * (hoverForce * clampedOutput);
                _rb.AddForceAtPosition(force, thruster.position, ForceMode.Acceleration);

                normalSum += hit.normal;
                hitCount++;
            }
            else
            {
                _pidControllers[i].Reset();
            }
        }

        if (hitCount > 0)
        {
            _averageGroundNormal = (normalSum / hitCount).normalized;
        }
        else
        {
            _averageGroundNormal = Vector3.up;
        }
    }

    private void HandleMovement()
    {
        Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(transform.forward, _averageGroundNormal).normalized;

        _rb.AddForce(slopeMoveDirection * _moveInput * moveSpeed, ForceMode.Acceleration);
        _rb.AddTorque(transform.up * _turnInput * turnSpeed, ForceMode.Acceleration);
    }
    
    private void Stabilize()
    {
        float angle = Vector3.Angle(transform.up, Vector3.up);
        
        if (angle > tiltAngleLimit)
        {
            Vector3 axis = Vector3.Cross(transform.up, Vector3.up);
            _rb.AddTorque(axis * (angle * 0.5f), ForceMode.Acceleration); 
        }
    }
    
    private void ApplyAirResistance()
    {
        if (!Physics.Raycast(transform.position, Vector3.down, hoverHeight * 2f, groundLayer))
        {
             _rb.AddForce(Vector3.down * 10f, ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        if (thrusters == null) return;
        
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.TransformPoint(centerOfMassOffset), 0.2f);
        
        Gizmos.color = Color.yellow;
        Vector3 slopeMoveDirection = Vector3.ProjectOnPlane(transform.forward, _averageGroundNormal).normalized;
        Gizmos.DrawLine(transform.position, transform.position + slopeMoveDirection * 3f);

        foreach (Transform thruster in thrusters)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(thruster.position, -transform.up * hoverHeight * 1.5f);
        }
    }
}
