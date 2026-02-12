using UnityEngine;

public class DroneController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 followOffset = new Vector3(2f, 2f, -2f);

    [Header("PID Settings")]
    [SerializeField, Range(0, 100)] private float kp = 10f;
    [SerializeField, Range(0, 50)] private float ki = 0f;
    [SerializeField, Range(0, 300)] private float kd = 25f;

    [Header("Movement Settings")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float maxForce = 100f;
    [SerializeField] private float rotationSpeed = 5f;

    private Rigidbody _rb;
    private readonly PID _pidX = new PID();
    private readonly PID _pidY = new PID();
    private readonly PID _pidZ = new PID();

    private void Awake()
    {
        if (!TryGetComponent(out _rb))
        {
            return;
        }

        _rb.useGravity = false; 
        _rb.linearDamping = 3f; 
        _rb.angularDamping = 5f; 
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        MoveDrone();
        RotateDrone();
    }

    private void MoveDrone()
    {
        Vector3 targetPos = target.position + target.TransformDirection(followOffset);
        Vector3 currentPos = transform.position;
        Vector3 error = targetPos - currentPos;

        float dt = Time.fixedDeltaTime;
        float x = _pidX.GetOutput(error.x, dt, kp, ki, kd);
        float y = _pidY.GetOutput(error.y, dt, kp, ki, kd);
        float z = _pidZ.GetOutput(error.z, dt, kp, ki, kd);

        Vector3 pidForce = new Vector3(x, y, z);
        pidForce = Vector3.ClampMagnitude(pidForce, maxForce);

        _rb.AddForce(pidForce, ForceMode.Acceleration);

        if (_rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            _rb.linearVelocity = _rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void RotateDrone()
    {
        Vector3 directionToTarget = target.position - transform.position;
        directionToTarget.y = 0; 

        if (directionToTarget.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            Quaternion nextRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
            _rb.MoveRotation(nextRotation);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        _pidX.Reset();
        _pidY.Reset();
        _pidZ.Reset();
    }
}
