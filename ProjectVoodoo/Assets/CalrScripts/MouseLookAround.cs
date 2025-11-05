using UnityEngine;

public class MouseLookAround : MonoBehaviour
{
    public float mouseSensitivity = 500f;
    float xRotation = 0f;

    public Transform playerBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        QualitySettings.vSyncCount = 1;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerBody.Rotate(Vector3.up * mouseX);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
