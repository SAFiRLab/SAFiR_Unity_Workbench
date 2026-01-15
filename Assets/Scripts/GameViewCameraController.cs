using UnityEngine;


[RequireComponent(typeof(Camera))]
public class GameViewCameraController : MonoBehaviour
{
    public float panSpeed = 0.3f;
    public float rotateSpeed = 3.0f;
    public float zoomSpeed = 10f;
    public float fastZoomMultiplier = 3f;

    private Vector3 lastMousePosition;

    void Update()
    {
        // Right Mouse Button held → rotate camera
        if (Input.GetMouseButton(1))
        {
            float rotX = Input.GetAxis("Mouse X") * rotateSpeed;
            float rotY = -Input.GetAxis("Mouse Y") * rotateSpeed;
            transform.eulerAngles += new Vector3(rotY, rotX, 0);
        }

        // Middle Mouse Button or Alt + Right Click → pan
        if (Input.GetMouseButton(2) || (Input.GetKey(KeyCode.LeftAlt) && Input.GetMouseButton(1)))
        {
            float moveX = -Input.GetAxis("Mouse X") * panSpeed;
            float moveY = -Input.GetAxis("Mouse Y") * panSpeed;
            transform.Translate(new Vector3(moveX, moveY, 0), Space.Self);
        }

        // Scroll Wheel → zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float multiplier = Input.GetKey(KeyCode.LeftShift) ? fastZoomMultiplier : 1f;
            transform.Translate(Vector3.forward * scroll * zoomSpeed * multiplier, Space.Self);
        }

        lastMousePosition = Input.mousePosition;
    }
}
