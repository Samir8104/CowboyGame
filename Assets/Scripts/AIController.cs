using System.Collections.Generic;
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
    
    private EnemyState state;
    [SerializeField] GameObject bulletPrefab;
    private bool hasCover;
    Vector3 Cover = Vector3.zero;

    private Vector3 coverVector;
    [Header("Nodes")]
    [SerializeField] List<Transform> navNodes = new List<Transform>();
    private Transform targetNode;
    [SerializeField] private Transform player;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 3.0f;
    [SerializeField] float radiusOfSatisfaction = 1.5f;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] float bulletCooldown = 1.5f;
   

    private void Start()
    {
        state = EnemyState.Roaming;
    }
    // Update is called once per frame
    void Update()
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
                if(isCloseToObject(player, 15f) || HasLOS(player))
                {
                    state = EnemyState.Attacking;
                }
                break;
            case EnemyState.Attacking:
                //check LOS before firing
                if (HasLOS(player))
                {
                    Debug.Log("Has LOS, firing at player");
                    Vector3 direction = (player.position - transform.position).normalized;
                    //transform.rotation = Quaternion.LookRotation(direction);

 
                    if(bulletCooldown > 0)
                    {
                        bulletCooldown -= Time.deltaTime;
                        if (!hasCover)
                        {
                            int coverDistance = 0; // The AI wants cover, but they need to find cover that is somewhat close to the player but out of LOS of the player
                            Vector3 closestCover; 
                            Cover = Vector3.zero;
                            for (int i = 0; i < 12; i++)
                            {
                                Cover = CheckForCover(15f);
                                if (Cover != Vector3.zero)
                                {
                                    hasCover = true;
                                    state = EnemyState.MovingToCover;
                                    break;
                                } // Check for cover 12 times
                            }
                        }
                     
                     
                        return;
                    }

                    bulletCooldown = 1.5f;
                    GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                    
                    newBullet.transform.position += direction * bulletSpeed;
                }
                else if(!isCloseToObject(player, 15f))
                {
                    state = EnemyState.Roaming;
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

                break;


        }
    }



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

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
    private void Move(Transform target) 
    {
        if (target == null) return;
        Vector3 direction = (target.position - transform.position).normalized;

        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }
}
