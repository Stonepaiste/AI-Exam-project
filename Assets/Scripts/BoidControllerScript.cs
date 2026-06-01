using UnityEngine;

public class BoidControllerScript : MonoBehaviour
{
    [Header("Altitude")]
    public float minAltitude = 20f;
    public float maxAltitude = 30f;

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float turnSpeed = 3f;

    private Vector3 _wanderTarget;
    private float   _wanderTimer;
    private float   _wanderInterval;

    void Start()
    {
        Vector3 pos = transform.position;
        pos.y = Random.Range(minAltitude, maxAltitude);
        transform.position = pos;

        PickNewTarget();
    }

    void Update()
    {
        _wanderTimer += Time.deltaTime;

        if (_wanderTimer >= _wanderInterval || Vector3.Distance(transform.position, _wanderTarget) < 2f)
            PickNewTarget();

        transform.position = GetNextPosition(Time.deltaTime);

        Vector3 dir = _wanderTarget - transform.position;
        if (dir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), turnSpeed * Time.deltaTime);
    }

    public Vector3 GetNextPosition(float deltaTime)
    {
        return Vector3.MoveTowards(transform.position, _wanderTarget, moveSpeed * deltaTime);
    }

    private void PickNewTarget()
    {
        _wanderTimer    = 0f;
        _wanderInterval = Random.Range(2f, 5f);

        float angle  = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float radius = Random.Range(15f, 40f);

        _wanderTarget = new Vector3(
            transform.position.x + Mathf.Cos(angle) * radius,
            Random.Range(minAltitude, maxAltitude),
            transform.position.z + Mathf.Sin(angle) * radius
        );
    }
}