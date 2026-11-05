using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AIController : MonoBehaviour
{

    //Pseudocode:
    // The cowboy will use an awesome state machine with three states, Roaming, Attacking, Retreating
    // The states are pretty self explanatory
    // In roaming, the cowboy will roam around some pre determined nodes (or it could be random, we'll see)
    // In the attack state, the ai will try and get line of sight with the player to attack them
    // If the cowboy gets hurt, it will try and retreat
    private enum EnemyState
    {
        Roaming,
        Attacking,
        MovingToCover,
        InCover,
        Retreating,
        Died
    }
    private Rigidbody rb;
    private EnemyState state;
    [SerializeField] GameObject bulletPrefab;
    private bool hasCover;
    Vector3 Cover = Vector3.zero;
    [SerializeField] int currentAmmo;

    private Vector3 coverVector;
    [Header("Nodes")]
    [SerializeField] List<Transform> navNodes = new List<Transform>();
    private Transform targetNode;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 3.0f;
    [SerializeField] float radiusOfSatisfaction = 1.5f;
    [SerializeField] float reloadSpeed = 3f;
    private float reloadTimer;
    [SerializeField] int ammo = 6;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] float bulletCooldown = 1.5f;
    [SerializeField] float detectionRange;
    private float timeSincePlrLastSeen = 0f;
    [SerializeField] float secondsToLoseAggro = 5f;

    [Header("Avoidance Settings")]
    [SerializeField] LayerMask obstacleLayer;
    [SerializeField] float sensorLength = 5f;
    [SerializeField] float sideSensorLength = 3f;
    [SerializeField] float sideSensorAngle = 30f;
    [SerializeField] float avoidanceForce = 10f;



    private void Start()
    {
        reloadTimer = reloadSpeed;
        currentAmmo = ammo;
        state = EnemyState.Roaming;
        rb = GetComponent<Rigidbody>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        switch (state)
        {
            case EnemyState.Roaming:
                if(targetNode == null)
                {
                  targetNode = GetRandomNode();
                }
                Move(targetNode);
                if (isCloseToObject(targetNode, radiusOfSatisfaction))
                {
                    targetNode = null;
                }
                if(isCloseToObject(player, detectionRange) && HasLOS(player))
                {
                    state = EnemyState.Attacking;
                }
                break;
            case EnemyState.Attacking:
                if (!isCloseToObject(player, detectionRange))
                {
                    Debug.Log("Far away from player, switching to roaming");
                    state = EnemyState.Roaming;
                    break;
                }
                if (HasLOS(player))
                {
                    timeSincePlrLastSeen = 0f;
                    Debug.Log("Has LOS, firing at player");
                    Vector3 direction = (player.position - transform.position).normalized;
                    //transform.rotation = Quaternion.LookRotation(direction);

                    
                    if(bulletCooldown >= 0) // Still on cooldown
                    {
                        
                        bulletCooldown -= Time.deltaTime;
                        break;
                    }
                    else
                    {
                        ShootPlayer(direction);
                        bulletCooldown = 1.5f; // reset cooldown

                    }
                }
                else
                {
                    //No LOS
                    if(timeSincePlrLastSeen >= secondsToLoseAggro) // The AI hasnt seen the player in a couple seconds, go back to roaming
                    {
                        state = EnemyState.Roaming;
                        timeSincePlrLastSeen = 0f;
                    }
                    else
                    {
                        timeSincePlrLastSeen += Time.deltaTime;
                    }
                }
                    break;
            case EnemyState.MovingToCover:
                if (Cover != Vector3.zero)
                {
                    if (isCloseToObject(Cover, radiusOfSatisfaction))
                    {
                        hasCover = false;
                        state = EnemyState.InCover;
                    }
                    else
                    {
                        Move(Cover);
                    }
                }
                else
                {
                    Debug.Log("No cover, finding areas around with cover");
                    for (int i = 0; i > 12; i++) // Check for cover within a radius of the AI. The for loop checks multiple spots within that radius around the AI
                    {
                        Cover = CheckForCover(7f);
                        if(Cover != Vector3.zero) // found cover so break the loop
                        {
                            break;
                        }
                    }
                    if(Cover == Vector3.zero)
                    {
                        RunAway(10);
                        // we want to move away but not move TOO far away
                        if(!isCloseToObject(player, 10))
                        {
                            state = EnemyState.InCover;
                        }
                    }
                }

                    break;
            case EnemyState.InCover:
                if(reloadTimer > 0)
                {
                    reloadTimer -= Time.deltaTime;
                    break;
                }
                else
                {
                    Debug.Log("Switching to attack state");
                    currentAmmo = ammo;
                    reloadTimer = reloadSpeed;
                    state = EnemyState.Attacking;
                    Cover = Vector3.zero;
                }
                break;


        }
    }

    private void RunAway(int distance)
    {
        Vector3 direction = (transform.position - player.position).normalized;
        Vector3 targetPos = (direction * distance) + transform.position;
        targetPos.y = transform.position.y; // prevent the AI from moving up/down
        Move(targetPos);

    
    }

    private void ShootPlayer(Vector3 direction)
    {
        if (currentAmmo != 0) // AI still has bullets so fire
        {
            currentAmmo -= 1;
            GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
            newBullet.transform.position += direction * bulletSpeed;
        }
        else
        {
            state = EnemyState.MovingToCover;
        }
    }
    // THE FIND COVER FUNCTION IS BROKEN ASF RIGHT NOW, IDK WHY
    private Vector3 CheckForCover(float radius)
    {
        Vector2 AIPosition = new Vector2(transform.position.x, transform.position.z);
        Vector2 random2dPoint = AIPosition + (Random.insideUnitCircle.normalized * radius); // Take a random 2d point at the AI's position
        Vector3 Cover = new Vector3(random2dPoint.x, transform.position.y, random2dPoint.y);
        RaycastHit hit;
        Vector3 directionToTarget = (player.transform.position - Cover);

        if(Physics.Raycast(Cover, directionToTarget, out hit, 50f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return Vector3.zero; // No cover found, keep moving
            }
            else
            {
                return Cover;
            }
        }
        return Vector3.zero;


    }
 
    private Transform GetRandomNode()
    {
        int RandomNum = Random.Range(0, navNodes.Count);
        return navNodes[RandomNum];
    }

    private bool isCloseToObject(Transform Object, float range)
    {
        Vector3 DistanceVector = Object.position - transform.position;
        if(DistanceVector.magnitude < range)
        {
            return true; // We are CLOSE to the target and winning
        }
        else
        {
            return false;
        }
    }

    private bool isCloseToObject(Vector3 position, float range) // We override our functions like true chads
    {
        Vector3 DistanceVector = position - transform.position;
        if (DistanceVector.magnitude < range)
        {
            return true; 
        }
        else
        {
            return false;
        }
    }

    private bool HasLOS(Transform player)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (player.position - transform.position);

        if(Physics.Raycast(transform.position, directionToTarget, out hit, 50f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }

        return false;
    }
    private void Move(Vector3 target)
    {
        if (target == null) return;
        Vector3 direction = (target - transform.position).normalized;
        Vector3 newDirection = CalculateAvoidance(direction);
        if (newDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(newDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));
        }

        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }
    private void Move(Transform target) 
    {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 newDirection = CalculateAvoidance(direction);
        if (newDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(newDirection);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

        }

        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    Vector3 CalculateAvoidance(Vector3 desiredDirection)
    {
        Vector3 steering = Vector3.zero;
        bool obstacleHit = false;
        
        // defining all the three rays
        Vector3 origin = transform.position + transform.forward * 0.5f; // forward front facing ray
        Vector3 leftRayDirection = Quaternion.Euler(0, -sideSensorAngle, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, sideSensorAngle, 0) * transform.forward;

        RaycastHit hit;

        // if the forward ray hits something, steer away from that
        if(Physics.Raycast(origin, transform.forward, out hit, sensorLength, obstacleLayer))
        {
            obstacleHit = true;
            steering += hit.normal * avoidanceForce;
            Debug.DrawLine(origin, hit.point, Color.red);
        }
        else
        {
            Debug.DrawRay(origin, transform.forward * sensorLength, Color.green);
        }

        if (Physics.Raycast(origin, leftRayDirection, out hit, sideSensorLength, obstacleLayer))
        {
            obstacleHit = true;
            steering += transform.right * avoidanceForce * (1 - (hit.distance / sideSensorLength)); // adds a push to the agent in the opposite direction, the 1-(hit.distance / sideSensorLength) is a weight which scales depending on the distance to the wall
            Debug.DrawLine(origin, hit.point, Color.red);
        }
        else
        {
            Debug.DrawRay(origin, leftRayDirection * sideSensorLength, Color.green);
        }

        if(Physics.Raycast(origin, rightRayDirection, out hit, sideSensorLength, obstacleLayer))
        {
            obstacleHit = true;
            steering -= transform.right * avoidanceForce * (1 - (hit.distance / sideSensorLength)); // same logic but negative!
            Debug.DrawLine(origin, hit.point, Color.red);

        }
        else
        {
            Debug.DrawRay(origin, rightRayDirection * sideSensorLength, Color.green);
        }

        if (obstacleHit)
        {
            return (desiredDirection + steering).normalized;
        }

        return desiredDirection;


    }
}
