using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletMovement : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private float bulletSpeed;
    [SerializeField] private float curveSpeed;

    private Vector3 targetPosition;
    private float radiusOfSatisfaction = 0.5f;
    
    void Start()
    {
        Destroy(gameObject, 5.0f);
        targetPosition = GameObject.FindWithTag("Enemy").transform.position;
    }

    void Update()
    {
        if (targetPosition != null)
        {
            KinematicArrive();
        }
        else
        {
            Vector3 newPosition = myTransform.position;
            newPosition += myTransform.forward * bulletSpeed * Time.deltaTime;
            myTransform.position = newPosition;
        }
    }

    private void KinematicArrive()
    {
        Vector3 towardsTarget = targetPosition - myTransform.position;

        if (towardsTarget.magnitude <= radiusOfSatisfaction)
        {
            Destroy(gameObject);
        }

        towardsTarget = towardsTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(towardsTarget);
        myTransform.rotation = Quaternion.Lerp(myTransform.rotation, targetRotation, curveSpeed);

        Vector3 newPosition = myTransform.position;
        newPosition += myTransform.forward * bulletSpeed * Time.deltaTime;
        myTransform.position = newPosition;
    }
}
