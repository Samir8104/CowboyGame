using UnityEngine;
using System.Collections.Generic;

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
        Retreating,
        Died
    }
    
    private EnemyState state;
    [SerializeField] GameObject bulletPrefab;
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
                if(isCloseToObject(player, 10f))
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
                    transform.rotation = Quaternion.Euler(direction);
                    
                    if(bulletCooldown > 0)
                    {
                        bulletCooldown -= Time.deltaTime;
                        return;
                    }
                    bulletCooldown = 1.5f;
                    GameObject newBullet = Instantiate(bulletPrefab, transform.position, transform.rotation);
                    
                    newBullet.transform.position += direction * bulletSpeed; 
                }
                break;
        }
    }

    private void MoveBullet(GameObject bullet)
    {
        bullet.transform.position += bullet.transform.position * bulletSpeed;

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

    private bool HasLOS(Transform player)
    {
        RaycastHit hit;
        Vector3 directionToTarget = (player.position - transform.position);

        if(Physics.Raycast(transform.position, directionToTarget, out hit, 50f))
        {
            return true;
        }

        return false;
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
