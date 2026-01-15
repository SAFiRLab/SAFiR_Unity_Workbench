using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Geometry;
using RosMessageTypes.Tf2;
using System;
using System.Collections.Generic;

public class TF2Broadcaster : MonoBehaviour
{
    private ROSConnection ros;

    public string worldFrame = "world";
    public string baseLinkFrame = "base_link";
    public string lidarFrame = "velodyne_link";

    public Transform baseLink;
    public Transform lidar;

    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<TFMessageMsg>("/tf");

        InvokeRepeating(nameof(PublishTF), 0.0f, 0.1f); // 5 Hz
    }

    void PublishTF()
    {
        double sec = Time.timeAsDouble;
        uint nanosec = (uint)((sec - Math.Floor(sec)) * 1e9);

        // Collect all transforms
        List<TransformStampedMsg> tfList = new List<TransformStampedMsg>();

        // 1. world -> base_link
        tfList.Add(CreateGlobalTF(worldFrame, baseLinkFrame, baseLink, (int)sec, nanosec));

        // 2. base_link -> lidar_link
        tfList.Add(CreateLocalTF(baseLinkFrame, lidarFrame, lidar, (int)sec, nanosec));

        // Create TFMessage and publish
        TFMessageMsg tfMessage = new TFMessageMsg(tfList.ToArray());

        // Show tf message in console for debugging
        Debug.Log($"Publishing TF: {tfMessage}");
        ros.Publish("/tf", tfMessage);
    }

    TransformStampedMsg CreateGlobalTF(string parent, string child, Transform tf, int sec, uint nanosec)
    {
        Vector3 pos = tf.position;
        Quaternion rot = tf.rotation;

        // Unity to ROS axis conversion
        Vector3 rosPos = new Vector3(pos.z, pos.x, pos.y);
        Quaternion rosRot = new Quaternion(rot.z, rot.x, -rot.y, rot.w);

        return new TransformStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = parent,
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = sec,
                    nanosec = nanosec
                }
            },
            child_frame_id = child,
            transform = new TransformMsg
            {
                translation = new Vector3Msg(rosPos.x, rosPos.y, rosPos.z),
                rotation = new QuaternionMsg(rosRot.x, rosRot.y, rosRot.z, rosRot.w)
            }
        };
    }

    TransformStampedMsg CreateLocalTF(string parent, string child, Transform tf, int sec, uint nanosec)
    {
        Vector3 pos = tf.localPosition;
        Quaternion rot = tf.localRotation;

        // Unity to ROS axis conversion
        Vector3 rosPos = new Vector3(pos.z, pos.x, pos.y);
        Quaternion rosRot = new Quaternion(rot.z, rot.x, -rot.y, rot.w);

        return new TransformStampedMsg
        {
            header = new RosMessageTypes.Std.HeaderMsg
            {
                frame_id = parent,
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = sec,
                    nanosec = nanosec
                }
            },
            child_frame_id = child,
            transform = new TransformMsg
            {
                translation = new Vector3Msg(rosPos.x, rosPos.y, rosPos.z),
                rotation = new QuaternionMsg(rosRot.x, rosRot.y, rosRot.z, rosRot.w)
            }
        };
    }
}
