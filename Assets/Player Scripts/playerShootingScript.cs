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
            // Ensure it faces where the gun is facing
            Quaternion targetRotation = Quaternion.LookRotation(gunObject.transform.forward);
            // Creates the prefab at the gun's location (can be changed to exit the barrel proper)
            GameObject bullet = Instantiate(bulletPrefab, gunObject.transform.position, targetRotation);
        }
    }

    // For later use
    public void OnShoot(InputAction.CallbackContext context)
    {
        return;
    }
}
