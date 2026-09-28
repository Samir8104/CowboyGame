using UnityEngine;
using UnityEngine.InputSystem;

public class playerShootingScript : MonoBehaviour
{
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] private GameObject gunObject;

    private bool mouseHeld;

    void Start()
    {
        
    }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Quaternion targetRotation = Quaternion.LookRotation(gunObject.transform.forward);
            GameObject bullet = Instantiate(bulletPrefab, gunObject.transform.position, targetRotation);
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        return;
    }
}
