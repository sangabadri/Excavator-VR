using UnityEngine;
using UnityEngine.InputSystem;

public class CamMovement : MonoBehaviour
{
    private float moveSpeed;
    public float minSpeed = 1f;
    public float maxSpeed = 4f;
    public float mouseSensitivity = 0.75f;

    private bool isHeadMovement = true;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        MoveCamera();

        if (Input.GetKey(KeyCode.M))
        {
            isHeadMovement = !isHeadMovement;
        }
        if (isHeadMovement)
        {
            RotateCamera();
        }
    }

    void MoveCamera()
    {
        Vector3 input = Vector3.zero;

        bool isShift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        bool isCtrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

        if (isCtrl)
        {
            moveSpeed = minSpeed;
        }
        else
        {
            moveSpeed = maxSpeed;
        }

        if (Keyboard.current.upArrowKey.isPressed)
        {
            if (isShift)
            {
                input.z += 1;
            }
            else
            {
                input.y += 1;
            }
        }

        if (Keyboard.current.downArrowKey.isPressed)
        {
            if (isShift)
            {
                input.z -= 1;
            }
            else
            {
                input.y -= 1;
            }
        }

        if (Keyboard.current.leftArrowKey.isPressed)
            input.x -= 1;

        if (Keyboard.current.rightArrowKey.isPressed)
            input.x += 1;

        Vector3 move = transform.forward * input.y + transform.right * input.x + transform.up * input.z;
        transform.position += move * moveSpeed * Time.deltaTime;
    }

    void RotateCamera()
    {
        Vector2 mouse = Mouse.current.delta.ReadValue() * mouseSensitivity;

        float mouseX = mouse.x;

        transform.localRotation = Quaternion.Euler(0f, transform.localEulerAngles.y + mouseX, 0f);
    }
}
