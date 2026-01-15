using UnityEngine;

public class PIDController
{
    public float kp_ = 1f;
    public float ki_ = 0f;
    public float kd_ = 0f;

    private float integral = 0f;
    private float last_error = 0f;

    public float PIDUpdate(float target, float current, float deltaTime)
    {
        float error = target - current;
        integral += error * deltaTime;
        float derivative = (error - last_error) / deltaTime;
        //Debug.Log($"PID Debug - Error: {error}, Integral: {integral}, Derivative: {derivative}");
        last_error = error;

        return kp_ * error + ki_ * integral + kd_ * derivative;
    }

    public void Reset()
    {
        integral = 0f;
        last_error = 0f;
    }
}

