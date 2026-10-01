using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float curveSpeed;
    [SerializeField] private float initialCurveFactor;

    [SerializeField] private float viewDistance;
    [SerializeField] private float viewAngle;

    private Vector3 targetPosition;
    private bool bending;
    private bool enemyTracked;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = GameObject.FindWithTag("Player").transform;
        // Flags
        bending = false;
        enemyTracked = false;
        // Deletes after 3 seconds
        Destroy(gameObject, 3.0f);
        if (GameObject.FindWithTag("Enemy") != null)
        {
            // Sets target position if enemy exists and checks for initial LoS
            targetPosition = GameObject.FindWithTag("Enemy").transform.position;
            if (HasLineOfSight())
            {
                // If LoS passes, start timer to bend
                StartCoroutine(BeginBending(0.1f));
                enemyTracked = true;
            }
        }
    }

    void Update()
    {
        // If the initial flag was true, enemy must exist within LoS
        if (enemyTracked)
        {
            // Continue tracking enemy position every frame
            targetPosition = GameObject.FindWithTag("Enemy").transform.position;
            // If the timer passed, start bending towards enemy
            if (bending == true)
            {
                KinematicArrive();
            }
            // Continue forward while timer passes
            else
            {
                Move();
            }
        }
        // If initial check didn't pass, move without rotating
        else
        {
            Move();
        }
    }

    private void Move()
    {
        // Basic moving forward method
        Vector3 newPosition = myTransform.position;
        newPosition += myTransform.forward * bulletSpeed * Time.deltaTime;
        myTransform.position = newPosition;
    }

    private void KinematicArrive()
    {
        // The usual, using inspector-set curve speeds to help rotation interpolation
        Vector3 towardsTarget = targetPosition - myTransform.position;

        towardsTarget = towardsTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(towardsTarget);
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, curveSpeed);

        Move();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // If you collide with a wall or the ground, delete the bullet
        if (collision.gameObject.tag == "Obstacle")
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.tag == "Enemy")
        {
            // If it's an enemy, do something before deleting
            Debug.Log("Hit!");
            Destroy(gameObject);
        }
    }

    private IEnumerator BeginBending(float time)
    {
        // Wait before bending
        yield return new WaitForSeconds(time);

        // Bool to allow continuous bending after
        bending = true;

        // One frame of Kinematic Arrive using the stronger initialCurveFactor to create a sharp bend
        Vector3 towardsTarget = targetPosition - myTransform.position;
        towardsTarget = towardsTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(towardsTarget);
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, initialCurveFactor);

        Vector3 newPosition = myTransform.position;
        newPosition += myTransform.forward * bulletSpeed * Time.deltaTime;
        myTransform.position = newPosition;
    }

    public bool HasLineOfSight()
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
}
