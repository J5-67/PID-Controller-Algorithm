[System.Serializable]
public class PID
{
    private float _lastError;
    private float _integral;

    public float GetOutput(float currentError, float deltaTime, float kp, float ki, float kd)
    {
        _integral += currentError * deltaTime;
        float derivative = (currentError - _lastError) / deltaTime;
        _lastError = currentError;

        return (currentError * kp) + (_integral * ki) + (derivative * kd);
    }

    public void Reset()
    {
        _lastError = 0f;
        _integral = 0f;
    }
}
