using UnityEngine;

public class HeadRotation : MonoBehaviour
{

    public float mouseSensitivity = 500f;

    public float xClamp = 50f;
    public float yClamp = 120f;

    private float xRotation = 0f;
    private float YRotation = 0f;

    private void Start()
    {
        //Locking the cursor to the middle of the screen and making it invisible
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        //control rotation around x axis (Look up and down)
        xRotation -= mouseY;

        //we clamp the rotation so we cant Over-rotate (like in real life)
        xRotation = Mathf.Clamp(xRotation, -xClamp, xClamp);

        //control rotation around y axis (Look up and down)
        YRotation += mouseX;
        YRotation = Mathf.Clamp(YRotation, -yClamp, yClamp);

        //applying both rotations
        transform.localRotation = Quaternion.Euler(xRotation, YRotation, 0f);

    }
}