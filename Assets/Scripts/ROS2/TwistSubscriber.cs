using UnityEngine;
using Unity.Robotics.ROSTCPConnector.ROSGeometry;
using RosMessageTypes.Geometry;
using Unity.Robotics.ROSTCPConnector;
using Unity.VisualScripting;

public class TwistSubscriber : MonoBehaviour
{
    ROSConnection ros;
    [SerializeField]
    public string topic_name_ = "/cmd_vel";
    public Vector3 linear_velocity_;
    public Vector3 angular_velocity_;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.Subscribe<TwistMsg>(topic_name_, TwistCallback);
    }

    // Callback function to handle incoming Twist messages
    void TwistCallback(TwistMsg msg)
    {
        if (msg == null)
        {
            Debug.LogWarning("Received null Twist message.");
            return;
        }

        // You can now use twist_msg_ to access the linear and angular velocities
        linear_velocity_ = new Vector3((float)msg.linear.x, (float)msg.linear.y, (float)msg.linear.z);
        angular_velocity_ = new Vector3((float)msg.angular.x, (float)msg.angular.y, (float)msg.angular.z);
    }
}
