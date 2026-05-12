using UnityEngine;

public class GhostCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float mouseSensitivity = 2f;
    public float boostMultiplier = 3f;

    float rotationX = 0f;
    float rotationY = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        CameraRotation();
        CameraMovement();
    }

    void CameraRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 100 * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 100 * Time.deltaTime;

        rotationX -= mouseY;
        rotationY += mouseX;

        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
    }

    void CameraMovement()
    {
        float speed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
            speed *= boostMultiplier;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        float y = 0f;
        if (Input.GetKey(KeyCode.E)) y = 1f;
        if (Input.GetKey(KeyCode.Q)) y = -1f;

        Vector3 move = transform.right * x + transform.forward * z + transform.up * y;

        transform.position += move * speed * Time.deltaTime;
    }
}