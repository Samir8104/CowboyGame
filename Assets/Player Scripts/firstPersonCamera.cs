using UnityEngine;
using UnityEngine.InputSystem;

public class firstPersonCamera : MonoBehaviour
{
    [Header("Sensitivity")]
    public float mouseSensitivity = 200f;

    [Header("Clamp mouse to screen")]
    [SerializeField] private float minY = -80f;
    [SerializeField] private float maxY = 80f;

    private float xRotation = 0f;
    private Vector2 lookInput;

    [Header("Toggle Viewpoint")]
    [SerializeField] private Vector3 firstPersonOffset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private Vector3 thirdPersonOffset = new Vector3(0.6f, 1.7f, -3f);
    [SerializeField] private float cameraTransitionSpeed = 8f;

    private bool isThirdPerson = false;
    private Vector3 currentOffset;


    [SerializeField] private Transform playerBody; 

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        currentOffset = firstPersonOffset;
        transform.localPosition = currentOffset;
    }

    void Update(){
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minY, maxY);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    void LateUpdate(){
    Vector3 targetOffset = isThirdPerson ? thirdPersonOffset : firstPersonOffset;

    currentOffset = Vector3.Lerp(currentOffset, targetOffset, cameraTransitionSpeed * Time.deltaTime);
    transform.localPosition = currentOffset;
}


    public void OnLook(InputAction.CallbackContext context){
        lookInput = context.ReadValue<Vector2>();
    }
    
    public void OnToggleView(InputAction.CallbackContext context){
    if (context.performed)
    {
        isThirdPerson = !isThirdPerson;
    }
    }
}
