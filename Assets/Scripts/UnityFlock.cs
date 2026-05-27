using UnityEngine;
using System.Collections;

public class UnityFlock : MonoBehaviour
{
    [Header("Movement")]
    public float minSpeed = 20.0f;
    public float turnSpeed = 20.0f;

    [Header("Random")]
    public float randomFreq = 20.0f;
    public float randomForce = 20.0f;

    [Header("Leader / Origin")]
    public float toOriginForce = 50.0f;
    public float toOriginRange = 100.0f;

    [Header("Forces")]
    public float gravity = 2.0f;

    [Header("Separation")]
    public float avoidanceRadius = 50.0f;
    public float avoidanceForce = 20.0f;

    [Header("Cohesion / Alignment")]
    public float followVelocity = 4.0f;
    public float followRadius = 40.0f;

    private Transform origin;
    private Vector3 velocity;
    public Vector3 normalizedVelocity { get; private set; }

    private Vector3 randomPush;
    private Vector3 originPush;

    private Transform[] objects;
    private UnityFlock[] otherFlocks;

    private Transform transformComponent;
    private float randomFreqInterval;

    void Start()
    {
        // Clamp critical inspector values to avoid division-by-zero / infinities.
        randomFreq = Mathf.Max(0.0001f, randomFreq);
        toOriginRange = Mathf.Max(0.0001f, toOriginRange);
        avoidanceRadius = Mathf.Max(0.0001f, avoidanceRadius);
        followRadius = Mathf.Max(0.0001f, followRadius);

        randomFreqInterval = 1.0f / randomFreq;

        origin = transform.parent;
        transformComponent = transform;

        // Get all UnityFlock components in this group (including self)
        UnityFlock[] tempFlocks = origin ? origin.GetComponentsInChildren<UnityFlock>() : new UnityFlock[] { this };

        objects = new Transform[tempFlocks.Length];
        otherFlocks = new UnityFlock[tempFlocks.Length];
        for (int i = 0; i < tempFlocks.Length; i++)
        {
            objects[i] = tempFlocks[i].transform;
            otherFlocks[i] = tempFlocks[i];
        }

        // Detach from parent (leader/controller)
        transform.parent = null;

        // IMPORTANT: initialize velocity so LookRotation never gets a zero vector.
        velocity = Random.onUnitSphere * minSpeed;
        normalizedVelocity = velocity.normalized;

        StartCoroutine(UpdateRandom());
    }

    IEnumerator UpdateRandom()
    {
        while (true)
        {
            randomPush = Random.insideUnitSphere * randomForce;

            yield return new WaitForSeconds(
                randomFreqInterval + Random.Range(
                    -randomFreqInterval / 2.0f,
                    randomFreqInterval / 2.0f));
        }
    }

    void Update()
    {
        Vector3 myPosition = transformComponent.position;

        // Accumulators across neighbors
        Vector3 sumVelocity = Vector3.zero;
        Vector3 sumPosition = Vector3.zero;
        Vector3 separation = Vector3.zero;
        int count = 0;

        for (int i = 0; i < objects.Length; i++)
        {
            Transform boidTransform = objects[i];
            if (boidTransform == null || boidTransform == transformComponent) continue;

            Vector3 otherPosition = boidTransform.position;

            // Cohesion uses average position
            sumPosition += otherPosition;

            // Alignment uses neighbors' velocity
            sumVelocity += otherFlocks[i] != null ? otherFlocks[i].normalizedVelocity : Vector3.zero;

            // Separation
            Vector3 forceV = myPosition - otherPosition;
            float dist = forceV.magnitude;

            if (dist > 0.000001f && dist < avoidanceRadius)
            {
                float forceMag = 1.0f - (dist / avoidanceRadius);
                separation += (forceV / dist) * forceMag * avoidanceForce;
            }

            count++;
        }

        Vector3 avgVelocity = Vector3.zero;
        Vector3 toAvg = Vector3.zero;

        if (count > 0)
        {
            // Alignment
            avgVelocity = (sumVelocity / count) * followVelocity;

            // Cohesion
            toAvg = (sumPosition / count) - myPosition;

            // Scale alignment influence by follow radius (optional weighting)
            float followMag = Mathf.Clamp01(toAvg.magnitude / followRadius);
            avgVelocity *= followMag;
        }

        // Pull to leader/origin
        originPush = Vector3.zero;
        if (origin != null)
        {
            Vector3 toLeader = origin.position - myPosition;
            float leaderDist = toLeader.magnitude;

            if (leaderDist > 0.000001f)
            {
                float leaderForceMag = leaderDist / toOriginRange;
                originPush = leaderForceMag * toOriginForce * (toLeader / leaderDist);
            }
        }

        // Maintain minimum speed once moving
        float speed = velocity.magnitude;
        if (speed > 0.000001f && speed < minSpeed)
            velocity = (velocity / speed) * minSpeed;

        // Build desired velocity
        Vector3 wantedVel = velocity;
        wantedVel -= wantedVel * Time.deltaTime;
        wantedVel += randomPush * Time.deltaTime;
        wantedVel += originPush * Time.deltaTime;
        wantedVel += avgVelocity * Time.deltaTime;
        wantedVel += separation * Time.deltaTime;
        if (toAvg.sqrMagnitude > 0.000001f)
            wantedVel += gravity * Time.deltaTime * toAvg.normalized;

        // Turn toward desired velocity
        velocity = Vector3.RotateTowards(velocity, wantedVel, turnSpeed * Time.deltaTime, 100.0f);

        // Safety: never move/rotate with NaN/Inf
        if (!IsFinite(velocity))
        {
            velocity = Random.onUnitSphere * minSpeed;
        }

        // Rotate only if velocity is non-zero
        if (velocity.sqrMagnitude > 0.000001f)
            transformComponent.rotation = Quaternion.LookRotation(velocity);

        // Move
        transformComponent.Translate(velocity * Time.deltaTime, Space.World);

        normalizedVelocity = (velocity.sqrMagnitude > 0.000001f) ? velocity.normalized : Vector3.zero;
    }

    private static bool IsFinite(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}

