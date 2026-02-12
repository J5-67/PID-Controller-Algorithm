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
        // 1. 현재 내 머리가 향하는 방향(Up)과 목표 방향(World Up)의 차이를 구하기
        Vector3 currentUp = transform.up;
        Vector3 targetUp = targetUpDirection.normalized;
        
        // 💡 중요 수정: Local Space가 아니라 World Space 기준으로 토크를 가해야 함!
        // 회전 축(Error Axis)을 구함
        Vector3 errorAxis = Vector3.Cross(currentUp, targetUp);

        // 2. PID 계산
        float dt = Time.fixedDeltaTime;
        float x = _pidX.GetOutput(errorAxis.x, dt, kp, ki, kd);
        float y = _pidY.GetOutput(errorAxis.y, dt, kp, ki, kd);
        float z = _pidZ.GetOutput(errorAxis.z, dt, kp, ki, kd);

        // 3. 토크 적용 (ForceMode에 주목!)
        // 💡 Global Torque를 사용해야 몸이 월드 기준(하늘)으로 바로 섬!!
        // Local relative torque가 들어가면 보드가 기울 때 같이 기울어짐.
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
