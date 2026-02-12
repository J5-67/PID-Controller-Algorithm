using UnityEngine;
using UnityEngine.Pool;

public class MissileController : MonoBehaviour
{
    [SerializeField] private float speed = 30f;
    [SerializeField] private float lifeTime = 5f;

    [Header("PID Settings")]
    [SerializeField, Range(0, 500)] private float kp = 200f;
    [SerializeField, Range(0, 100)] private float ki = 0f;
    [SerializeField, Range(0, 200)] private float kd = 25f;

    private Transform _target;
    private Rigidbody _rb;
    private IObjectPool<MissileController> _pool;
    private float _timer;

    private readonly PID _pidX = new PID();
    private readonly PID _pidY = new PID();
    private readonly PID _pidZ = new PID();

    private void Awake()
    {
        if (!TryGetComponent(out _rb)) return;
        
        _rb.useGravity = false;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 3f;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void OnEnable()
    {
        _timer = 0f;
        _pidX.Reset();
        _pidY.Reset();
        _pidZ.Reset();
        
        if (_rb != null)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
    }

    private void FixedUpdate()
    {
        _timer += Time.fixedDeltaTime;
        if (_timer >= lifeTime)
        {
            Release();
            return;
        }

        Move();
        Steer();
    }

    private void Move()
    {
        _rb.linearVelocity = transform.forward * speed;
    }

    private void Steer()
    {
        if (_target == null) return;

        Vector3 direction = (_target.position - transform.position).normalized;
        Vector3 error = Vector3.Cross(transform.forward, direction);

        float dt = Time.fixedDeltaTime;
        float x = _pidX.GetOutput(error.x, dt, kp, ki, kd);
        float y = _pidY.GetOutput(error.y, dt, kp, ki, kd);
        float z = _pidZ.GetOutput(error.z, dt, kp, ki, kd);

        Vector3 torque = new Vector3(x, y, z);
        _rb.AddTorque(torque, ForceMode.Acceleration);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"BOOM! Hit: {other.name}");
        Release();
    }

    public void SetTarget(Transform target) => _target = target;
    public void SetPool(IObjectPool<MissileController> pool) => _pool = pool;

    private void Release()
    {
        _pool?.Release(this);
    }
}
