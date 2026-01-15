using UnityEngine;


[System.Serializable]
public class Wheel
{
    public WheelCollider wheel_collider_;
    public Transform mesh_;
    [System.NonSerialized]
    public float forward_stiffness_ = 1.5f;
    [System.NonSerialized]
    public float side_stiffness_ = 2.0f;
    public PIDController pid_controller_ = new PIDController {kp_ = 5.0f, ki_ = 1.0f, kd_ = 0.0f};
    public float max_rpm_ = 76.0f; // Maximum RPM for the wheel
    public float wheel_inertia_ = 0.01f;

    [System.NonSerialized]
    private float max_linear_speed_;

    public void Initialize()
    {
        // Calculate maximum linear speed based on max RPM and wheel radius
        max_linear_speed_ = max_rpm_ * wheel_collider_.radius * Mathf.PI / 30f; // Convert RPM to m/s
        ApplyFriction();
    }

    public void UpdateWheel()
    {
        if (wheel_collider_ == null || mesh_ == null)
        {
            Debug.LogWarning("UpdateWheel skipped: WheelCollider or mesh not initialized.");
            return;
        }

        Vector3 position, tmp;
        Quaternion rotation;
        mesh_.GetPositionAndRotation(out position, out rotation);
        wheel_collider_.GetWorldPose(out tmp, out rotation);
        mesh_.position = position;
        mesh_.rotation = rotation;
    }

    public virtual void Action(float target_wheel_velocity_mps, float brake, float torque_scaling_factor)
    {
        if (wheel_collider_ == null)
        {
            Debug.LogError("WheelCollider is not initialized. Please call InitInstance() first.");
            return;
        }

        //ApplyFriction();
        WheelFrictionCurve sidewaysFriction = wheel_collider_.sidewaysFriction;
        WheelFrictionCurve forwardFriction = wheel_collider_.forwardFriction;
        Debug.Log($"Side Stiffness: {sidewaysFriction.stiffness}, Forward Stiffness: {forwardFriction.stiffness}");

        float deltaTime = Time.fixedDeltaTime;

        // Convert wheel rotational speed to linear speed
        float wheel_radius = wheel_collider_.radius;
        float current_angular_velocity = wheel_collider_.rotationSpeed * Mathf.Deg2Rad; // rad/s
        float current_linear_speed = current_angular_velocity * wheel_radius;

        // === PID speed control ===
        float correction = pid_controller_.PIDUpdate(target_wheel_velocity_mps, current_linear_speed, deltaTime);
        float control_speed = current_linear_speed + correction;
        float control_angular_velocity = control_speed / wheel_radius;
        float control_torque = control_angular_velocity * wheel_inertia_ * torque_scaling_factor / deltaTime;
        float max_torque = max_linear_speed_ / wheel_radius * wheel_inertia_ * torque_scaling_factor;
        control_torque = Mathf.Clamp(control_torque, -max_torque, max_torque);
        wheel_collider_.motorTorque = control_torque;

        Debug.Log($"Correction: {correction:F6}, Current Speed: {current_linear_speed:F6} m/s, Control Torque: {control_torque:F6} Nm");

        // === Direct torque based on target velocity (no PID) ===
        /*float target_angular_velocity = target_wheel_velocity_mps / wheel_radius;
        float target_torque = target_angular_velocity * wheel_inertia_ * torque_scaling_factor / deltaTime;
        wheel_collider_.motorTorque = target_torque;*/

        wheel_collider_.brakeTorque = brake;
        UpdateWheel();
    }

    public void ApplyFriction()
    {
        WheelFrictionCurve forwardFriction = wheel_collider_.forwardFriction;
        WheelFrictionCurve sidewaysFriction = wheel_collider_.sidewaysFriction;

        forwardFriction.stiffness = forward_stiffness_;
        sidewaysFriction.stiffness = side_stiffness_;

        wheel_collider_.forwardFriction = forwardFriction;
        wheel_collider_.sidewaysFriction = sidewaysFriction;
    }
    
}

