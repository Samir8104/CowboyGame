using UnityEngine;

public class enemyMoveTest : MonoBehaviour
{
    [SerializeField] private Transform myTransform;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float distance;

    private Vector3 startPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPos = myTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float movement = Mathf.PingPong(Time.time * moveSpeed, distance);
        myTransform.position = new Vector3(startPos.x + movement, startPos.y, startPos.z);
    }
}
