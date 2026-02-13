using UnityEngine;

public class ActiveRagdollBalance : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Vector3 targetUpDirection = Vector3.up;

    [Header("PID Settings")]
    [SerializeField, Range(0, 5000)] private float kp = 2000f;
    [SerializeField, Range(0, 100)] private float ki = 0f;
    [SerializeField, Range(0, 500)] private float kd = 150f;

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
        
        _rb.maxAngularVelocity = 50f; 
    }

    private void FixedUpdate()
    {
        ApplyBalance();
    }

    private void ApplyBalance()
    {
        Vector3 currentUp = transform.up;
        Vector3 targetUp = targetUpDirection.normalized;
        
        Vector3 errorAxis = Vector3.Cross(currentUp, targetUp);

        float dt = Time.fixedDeltaTime;
        float x = _pidX.GetOutput(errorAxis.x, dt, kp, ki, kd);
        float y = _pidY.GetOutput(errorAxis.y, dt, kp, ki, kd);
        float z = _pidZ.GetOutput(errorAxis.z, dt, kp, ki, kd);

        Vector3 torque = new Vector3(x, y, z);
        _rb.AddTorque(torque, ForceMode.Acceleration); 
    }

    public void SetTargetUp(Vector3 newUp)
    {
        targetUpDirection = newUp;
    }

    public void ResetPID()
    {
        _pidX.Reset();
        _pidY.Reset();
        _pidZ.Reset();
    }
}
