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

    [Header("Nodes")]
    [SerializeField] List<Transform> navNodes = new List<Transform>();
    private Transform target;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] float rotationSpeed = 3.0f;
    [SerializeField] float radiusOfSatisfaction = 1.5f;

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
                if(target == null)
                {
                  target = GetRandomNode();
                }
                Move(target);
                if (isCloseToTarget())
                {
                    target = null;
                }
                break;
        }
    }
    private Transform GetRandomNode()
    {
        int RandomNum = Random.Range(0, navNodes.Count + 1);
        return navNodes[RandomNum];
    }

    private bool isCloseToTarget()
    {
        Vector3 DistanceVector = target.position - transform.position;
        if(DistanceVector.magnitude < radiusOfSatisfaction)
        {
            return true; // We are CLOSE to the target and winning
        }
        else
        {
            return false;
        }
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
