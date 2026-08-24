using UnityEngine;
using UnityEngine.InputSystem;

public class CamRotation : MonoBehaviour
{
    public float mouseSensitivity = 0.75f;

    private float xRotation = 0f;
    private bool isHeadMovement = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.M))
        {
            isHeadMovement = !isHeadMovement;
        }
        if (isHeadMovement)
        {
            RotateCamera();
        }
    }

    void RotateCamera()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue() * mouseSensitivity;

        float mouseY = mouse.y;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -35f, 40f);


        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }
}
