using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class playerShootingScript : MonoBehaviour
{
    [Header("Objects of Interest")]
    [SerializeField] public GameObject bulletPrefab;
    [SerializeField] private GameObject gunObject;
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Gun Parameters")]
    [SerializeField] private float viewDistance;
    [SerializeField] private float viewAngle;
    [SerializeField] private float timeToLock; // in seconds
    [SerializeField] private float lockLinger; // in seconds
    [SerializeField] private float reloadSpeed; // in seconds
    [SerializeField] private float firingDelay; // in Rounds Per Minute (RPM)
    [SerializeField] private float maxAmmo;

    private Transform playerTransform;
    private GameObject enemy;
    private Vector3 targetPosition;
    private bool locked;
    private bool firingLocked;
    private bool reloading;
    private bool mouseHeld;
    private float timeSpentLocking = 0f;
    private float timeSpentLingering = 0f;
    private float timeSpentReloading = 0f;
    private float timeBetweenShots = 0f;
    private float ammoHeld = 0f;

    void Start()
    {
        // Flags
        locked = false;
        firingLocked = false;
        reloading = false;
        ammoHeld = maxAmmo;
        ammoText.text = "Ammo: " + ammoHeld;
        // Get the transforms of the player and enemy
        playerTransform = GameObject.FindWithTag("Player").transform;
        if (GameObject.FindWithTag("Enemy") != null)
        {
            enemy = GameObject.FindWithTag("Enemy");
        }
    }

    void Update()
    {
        // Constantly update target position
        targetPosition = enemy.transform.position;

        // Track time between shots for RPM and Locking purposes
        timeBetweenShots += Time.deltaTime;

        // If holding, not locked, and within LoS, start locking
        if (mouseHeld && !locked && HasLineOfSight())
        {
            StartLocking();
        }
        // If you're already locked and lose LoS, let the lock linger
        if (locked && (!HasLineOfSight() || timeBetweenShots > 1))
        {
            LockLinger();
        }
        if (firingLocked)
        {
            RPMDelay();
        }
        if (reloading)
        {
            Reload();
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            mouseHeld = true;
        }
        else if (context.canceled)
        {
            mouseHeld = false;
            // Only shoots bullets when the mouse button is released
            // Additionally, yield to RPM limit
            if (!firingLocked && ammoHeld > 0 && !reloading)
            {
                Shoot();
                timeBetweenShots = 0f;
                firingLocked = true;
                ammoHeld--;
                ammoText.text = "Ammo: " + ammoHeld;
            }
            // Resets lock progress; must be continuous
            timeSpentLocking = 0f;
        }
    }

    public void OnReload(InputAction.CallbackContext context)
    {
        if (context.started && ammoHeld < maxAmmo && !reloading)
        {
            reloading = true;
            gunObject.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
        }
    }

    private void Shoot()
    {
        // Ensure it faces where the gun is facing
        Quaternion targetRotation = Quaternion.LookRotation(gunObject.transform.forward);
        // Creates the prefab at the gun's location (can be changed to exit the barrel proper)
        GameObject bullet = Instantiate(bulletPrefab, gunObject.transform.position, targetRotation);
        // Sets the bulletMovement components enemyTracked flag to value of locked, as they are functionally the same
        bullet.GetComponent<bulletMovement>().enemyTracked = locked;
    }

    private void Reload()
    {
        // If you spent enough time, set the flag
        if (timeSpentReloading > reloadSpeed)
        {
            reloading = false;
            timeSpentReloading = 0f;
            ammoHeld = maxAmmo;
            ammoText.text = "Ammo: " + ammoHeld;
            gunObject.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.grey);
        }
        // Otherwise, keep adding time
        else
        {
            timeSpentReloading += Time.deltaTime;
        }
    }

    private void StartLocking()
    {
        // If you spent enough time, set the flag
        if (timeSpentLocking > timeToLock)
        {
            locked = true;
            enemy.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.red);
        }
        // Otherwise, keep adding time
        else
        {
            timeSpentLocking += Time.deltaTime;
        } 
    }

    // The same as StartLocking(), but with different parameters
    private void LockLinger()
    {
        if (timeSpentLingering > lockLinger)
        {
            locked = false;
            enemy.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.grey);
            timeSpentLingering = 0f;
        }
        else
        {
            timeSpentLingering += Time.deltaTime;
        }
    }

    // Similar to the other two, but with even more different parameters
    private void RPMDelay()
    {
        if (timeBetweenShots > (1 / (firingDelay / 60)))
        {
            firingLocked = false;
        }
    }

    public bool InRange()
    {
        // Checks distance from player
        Vector3 directionToTarget = targetPosition - playerTransform.position;
        float distanceToTarget = directionToTarget.magnitude;

        if (distanceToTarget > viewDistance)
            return false;

        // Checks angle of player in comparison to angle towards target
        float angleToTarget = Vector3.Angle(playerTransform.forward, directionToTarget);
        if (angleToTarget > viewAngle / 2f)
            return false;

        // If both pass, it's a valid target!
        return true;
    }

    public bool HasLineOfSight()
    {
        // If it's not even in the correct viewing range, don't bother
        if (!InRange())
        {
            return false;
        }

        RaycastHit hit;
        // Use a Linecast, since it already checked for distance within InRange()
        if (Physics.Linecast(playerTransform.position, targetPosition, out hit))
        {
            // If it hits the enemy, it's in LoS
            if (hit.collider.gameObject.tag == "Enemy")
            {
                return true;
            }
        }

        // If it didn't, it's not in LoS
        return false;
    }

}
