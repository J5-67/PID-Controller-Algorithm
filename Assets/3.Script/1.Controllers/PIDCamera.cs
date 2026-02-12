using UnityEngine;

public class PIDCamera : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 3, -6);
    [SerializeField] private float lookSpeed = 10f;

    [Header("PID Settings")]
    [SerializeField, Range(0, 20)] private float kp = 5f;
    [SerializeField, Range(0, 10)] private float ki = 0f;
    [SerializeField, Range(0, 10)] private float kd = 1f;

    [Header("Safety Settings")]
    [SerializeField] private float maxSpeed = 50f;

    private readonly PID _pidX = new PID();
    private readonly PID _pidY = new PID();
    private readonly PID _pidZ = new PID();

    private void Start()
    {
        ResetCamera();
    }

    private void LateUpdate()
    {
        if (target == null || Time.deltaTime < 0.0001f) return;

        Vector3 targetPos = target.TransformPoint(offset);
        Vector3 currentPos = transform.position;
        Vector3 error = targetPos - currentPos;

        float dt = Time.deltaTime;
        float x = _pidX.GetOutput(error.x, dt, kp, ki, kd);
        float y = _pidY.GetOutput(error.y, dt, kp, ki, kd);
        float z = _pidZ.GetOutput(error.z, dt, kp, ki, kd);

        Vector3 velocity = new Vector3(x, y, z);
        velocity = Vector3.ClampMagnitude(velocity, maxSpeed);

        transform.position += velocity * dt;

        Vector3 direction = target.position - transform.position;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, lookSpeed * dt);
        }
    }

    public void ResetCamera()
    {
        if (target == null) return;
        
        transform.position = target.TransformPoint(offset);
        transform.LookAt(target);
        
        _pidX.Reset();
        _pidY.Reset();
        _pidZ.Reset();
    }
}
