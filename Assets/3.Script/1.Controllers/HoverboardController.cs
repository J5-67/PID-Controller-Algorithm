using UnityEngine;

public class HoverboardController : MonoBehaviour
{
    [SerializeField] private float hoverHeight = 1.5f;
    [SerializeField] private float hoverForce = 1500f;
    
    [SerializeField, Range(0, 10)] private float kp = 5f;
    [SerializeField, Range(0, 10)] private float ki = 0f;
    [SerializeField, Range(0, 10)] private float kd = 2f;

    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float turnSpeed = 2f;

    [SerializeField] private Transform[] thrusters;

    [SerializeField] private LayerMask groundLayer;

    private Rigidbody _rb;
    private PID[] _pidControllers;

    private void Awake()
    {
        if (!TryGetComponent(out _rb))
        {
            Debug.LogError("HoverboardController: Rigidbody not found!");
            return;
        }

        _pidControllers = new PID[thrusters.Length];
        for (int i = 0; i < thrusters.Length; i++)
        {
            _pidControllers[i] = new PID();
        }
    }

    private void FixedUpdate()
    {
        HandleHover();
        HandleMovement();
    }

    private void HandleHover()
    {
        for (int i = 0; i < thrusters.Length; i++)
        {
            Transform thruster = thrusters[i];
            
            // Raycast now only hits the specified groundLayer
            if (Physics.Raycast(thruster.position, -transform.up, out RaycastHit hit, hoverHeight * 1.5f, groundLayer))
            {
                float currentHeight = hit.distance;
                float error = hoverHeight - currentHeight;

                float output = _pidControllers[i].GetOutput(error, Time.fixedDeltaTime, kp, ki, kd);
                
                // Thrusters can only push UP, never pull down! (Prevent vacuum effect)
                // Also, limit the max force if needed, but for now just prevent negative.
                float clampedOutput = Mathf.Max(0f, output);

                // Use global Up vector to ensure force is always applied upwards
                Vector3 force = Vector3.up * (hoverForce * clampedOutput);
                
                // Use ForceMode.Acceleration to ignore mass
                _rb.AddForceAtPosition(force, thruster.position, ForceMode.Acceleration);

                // Debug log (Comment out if too spammy)
                // if (i == 0) Debug.Log($"[Thruster 0] Error: {error:F2}, PID Out: {output:F2}, Clamped Force: {force.y:F2}");
            }
            else
            {
                _pidControllers[i].Reset();
            }
        }
    }

    private void HandleMovement()
    {
        float move = Input.GetAxis("Vertical");
        float turn = Input.GetAxis("Horizontal");

        _rb.AddForce(transform.forward * move * moveSpeed, ForceMode.Acceleration);
        _rb.AddTorque(transform.up * turn * turnSpeed, ForceMode.Acceleration);
    }

    private void OnDrawGizmos()
    {
        if (thrusters == null) return;

        foreach (Transform thruster in thrusters)
        {
            if (thruster == null) continue;
            
            Gizmos.color = Color.red;
            Vector3 direction = -transform.up * hoverHeight * 1.5f;
            Gizmos.DrawRay(thruster.position, direction);

            if (Physics.Raycast(thruster.position, -transform.up, out RaycastHit hit, hoverHeight * 1.5f, groundLayer))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(hit.point, 0.1f);
                Gizmos.DrawLine(thruster.position, hit.point);
            }
        }
    }
}
