using System.Collections.Generic;
using UnityEngine;
using UnitySensors.Sensor;

public class Vehicle : MonoBehaviour
{
    [SerializeField]
    public List<Wheel> left_wheels_ = new List<Wheel>();
    [SerializeField]
    public List<Wheel> right_wheels_ = new List<Wheel>();
    public TwistSubscriber twist_subscriber_;
    public Rigidbody rb;
    public float torque_scaling_factor_;
    public float wheel_base_ = 0.555f; // Distance between left and right wheels
    public float brake_force_ = 10f;
    public float max_acceleration_ = 2.0f;
    private bool was_key_pressed_ = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        for (int i = 0; i < left_wheels_.Count; i++)
        {
            if (left_wheels_[i] != null)
                left_wheels_[i].Initialize();
        }

        for (int i = 0; i < right_wheels_.Count; i++)
        {
            if (right_wheels_[i] != null)
                right_wheels_[i].Initialize();
        }

    }

    private void FixedUpdate()
    {
        if (twist_subscriber_ == null)
        {
            Debug.LogWarning("TwistSubscriber not assigned to Vehicle.");
            return;
        }

        // Manual control for debugging
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (twist_subscriber_.linear_velocity_.z == 0.0f)
                twist_subscriber_.linear_velocity_.z = 0.1f;

            if (twist_subscriber_.linear_velocity_.z < 0.0f)
                twist_subscriber_.linear_velocity_.z += 0.15f;

            twist_subscriber_.linear_velocity_.z += 0.1f;
            if (twist_subscriber_.linear_velocity_.z > 1.0f)
                twist_subscriber_.linear_velocity_.z = 1.0f;
                
            was_key_pressed_ = true; // Set key pressed state
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            if (twist_subscriber_.linear_velocity_.z == 0.0f)
                twist_subscriber_.linear_velocity_.z = -0.1f;

            if (twist_subscriber_.linear_velocity_.z > 0.0f)
                twist_subscriber_.linear_velocity_.z -= 0.15f;

            twist_subscriber_.linear_velocity_.z -= 0.1f;
            if (twist_subscriber_.linear_velocity_.z < -1.0f)
                twist_subscriber_.linear_velocity_.z = -1.0f;

            was_key_pressed_ = true; // Set key pressed state
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            if (twist_subscriber_.angular_velocity_.y == 0.0f)
                twist_subscriber_.angular_velocity_.y = -0.1f;

            if (twist_subscriber_.angular_velocity_.y > 0.0f)
                twist_subscriber_.angular_velocity_.y = 0.0f;

            twist_subscriber_.angular_velocity_.y -= 0.1f;
            if (twist_subscriber_.angular_velocity_.y < -1.0f)
                twist_subscriber_.angular_velocity_.y = -1.0f;
                
            was_key_pressed_ = true; // Set key pressed state
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            if (twist_subscriber_.angular_velocity_.y == 0.0f)
                twist_subscriber_.angular_velocity_.y = 0.1f;

            if (twist_subscriber_.angular_velocity_.y < 0.0f)
                twist_subscriber_.angular_velocity_.y = 0.0f;

            twist_subscriber_.angular_velocity_.y += 0.1f;
            if (twist_subscriber_.angular_velocity_.y > 1.0f)
                twist_subscriber_.angular_velocity_.y = 1.0f;

            was_key_pressed_ = true; // Set key pressed state
        }

        // If no key is pressed, slow down the vehicle
        if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow) &&
            !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow) &&
            was_key_pressed_)
        {
            twist_subscriber_.linear_velocity_.z *= 0.8f; // Slow down gradually
            twist_subscriber_.angular_velocity_.y *= 0.8f; // Slow down rotation gradually

            bool stopped_linear = false;
            bool stopped_angular = false;
            if (Mathf.Abs(twist_subscriber_.linear_velocity_.z) < 0.01f)
            {
                twist_subscriber_.linear_velocity_.z = 0f; // Stop if very slow
                stopped_linear = true;
            }
            if (Mathf.Abs(twist_subscriber_.angular_velocity_.y) < 0.01f)
            {
                twist_subscriber_.angular_velocity_.y = 0f; // Stop if very slow
                stopped_angular = true;
            }
            if (stopped_linear && stopped_angular)
            {
                was_key_pressed_ = false; // Reset key pressed state
                foreach (var wheel in left_wheels_)
                    wheel.pid_controller_.Reset();
                foreach (var wheel in right_wheels_)
                    wheel.pid_controller_.Reset();
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            twist_subscriber_.linear_velocity_.z = 0f;
            twist_subscriber_.angular_velocity_.y = 0f;
            was_key_pressed_ = false; // Reset key pressed state
            foreach (var wheel in left_wheels_)
                wheel.pid_controller_.Reset();
            foreach (var wheel in right_wheels_)
                wheel.pid_controller_.Reset();
        }
        

        // Extract values from the subscribed twist message
        float linear_vel = twist_subscriber_.linear_velocity_.z;
        float angular_vel = twist_subscriber_.angular_velocity_.y;


        if (angular_vel != 0.0f)
        {
            foreach (var wheel in left_wheels_)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheel_collider_.sidewaysFriction;
                sidewaysFriction.stiffness = 0.0f;
                wheel.wheel_collider_.sidewaysFriction = sidewaysFriction;
                Debug.Log("Left Wheel Side Stiffness set to 0.0f");
            }

            foreach (var wheel in right_wheels_)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheel_collider_.sidewaysFriction;
                sidewaysFriction.stiffness = 0.0f;
                wheel.wheel_collider_.sidewaysFriction = sidewaysFriction;
            }
        }
        else
        {
            foreach (var wheel in left_wheels_)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheel_collider_.sidewaysFriction;
                sidewaysFriction.stiffness = 2.0f;
                wheel.wheel_collider_.sidewaysFriction = sidewaysFriction;
                Debug.Log("Left Wheel Side Stiffness reset to 2.0f");
            }

            foreach (var wheel in right_wheels_)
            {
                WheelFrictionCurve sidewaysFriction = wheel.wheel_collider_.sidewaysFriction;
                sidewaysFriction.stiffness = 2.0f;
                wheel.wheel_collider_.sidewaysFriction = sidewaysFriction;
            }
        }

        // Compute individual wheel velocities (m/s)
        float left_wheel_velocity = linear_vel - (angular_vel * wheel_base_ / 2.0f);
        float right_wheel_velocity = linear_vel + ( angular_vel * wheel_base_ / 2.0f);
        //Debug.Log($"Left Wheel Velocity: {left_wheel_velocity:F6} m/s, Right Wheel Velocity: {right_wheel_velocity:F6} m/s");

        bool braking = Mathf.Abs(linear_vel) < 0.01f && Mathf.Abs(angular_vel) < 0.01f;
        float brake = braking ? brake_force_ : 0f;

        foreach (var wheel in left_wheels_)
        {
            wheel.Action(left_wheel_velocity, brake, torque_scaling_factor_);
        }
        foreach (var wheel in right_wheels_)
        {
            wheel.Action(right_wheel_velocity, brake, torque_scaling_factor_);
        }

        // Log the current forward speed for debugging
        Debug.Log($"Current Forward Speed: {GetCurrentForwardSpeed()} m/s");
    }

    public float GetCurrentForwardSpeed()
    {
        if (rb != null)
            return Vector3.Dot(rb.linearVelocity, transform.forward);
        return 0f;
    }
}
