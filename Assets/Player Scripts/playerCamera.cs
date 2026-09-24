using UnityEngine;
using UnityEngine.InputSystem;

public class playerCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 200f;

    [Header("Clamp mouse to screen")]
    [SerializeField] private float minY = -80f;
    [SerializeField] private float maxY = 80f;

    private float xRotation = 0f;
    private Vector2 lookInput;

    [Header("Toggle Viewpoint")]
    [SerializeField] private Vector3 firstPersonOffset;
    [SerializeField] private Vector3 gunOffset;
    [SerializeField] private float cameraTransitionSpeed = 8f;
    private Vector3 currentOffset;


    [SerializeField] private Transform playerBody;
    [SerializeField] private Transform gunBody;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentOffset = firstPersonOffset;
        transform.localPosition = currentOffset;
    }

    void Update()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void LateUpdate()
    {
        Vector3 targetOffset = firstPersonOffset;
        Vector3 targetGunOffset = gunOffset;

        currentOffset = Vector3.Lerp(currentOffset, targetOffset, cameraTransitionSpeed * Time.deltaTime);
        transform.localPosition = currentOffset;
        gunBody.transform.localPosition = currentOffset + targetGunOffset;
    }


    public void OnLook(InputAction.CallbackContext context)
    {
        lookInput = context.ReadValue<Vector2>();
    }

}
