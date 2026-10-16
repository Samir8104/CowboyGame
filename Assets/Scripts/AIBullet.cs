using UnityEngine;
using UnityEngine.UIElements;

public class AIBullet : MonoBehaviour
{

    private Vector3 targetPosition;
    [SerializeField] private float bulletSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetPosition = GameObject.FindWithTag("Player").transform.position;
        Destroy(gameObject, 3.0f);
        Vector3 towardsTarget = targetPosition - transform.position;

        towardsTarget = towardsTarget.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(towardsTarget);
        transform.rotation = targetRotation;
    }

    void Update()
    {
        Move();
    }

    private void Move()
    {
        // Basic moving forward method
        Vector3 newPosition = transform.position;
        newPosition += transform.forward * bulletSpeed * Time.deltaTime;
        transform.position = newPosition;
    }
}
