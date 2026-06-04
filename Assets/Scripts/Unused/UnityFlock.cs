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

    [Header("Obstacle Avoidance")]
    [Tooltip("Layers treated as obstacles (terrain, buildings, trees).")]
    public LayerMask obstacleMask;
    [Tooltip("How far ahead the boid looks for obstacles.")]
    public float lookAhead = 15.0f;
    [Tooltip("Radius of the sphere cast — bigger = wider safety margin.")]
    public float castRadius = 2.0f;
    [Tooltip("How hard the boid pushes away from obstacles.")]
    public float obstacleAvoidForce = 200.0f;
    [Tooltip("Minimum altitude — boids will get pushed up if they go below this.")]
    public float minAltitude = 10.0f;
    [Tooltip("Extra upward push strength when below minimum altitude.")]
    public float altitudeForce = 100.0f;

    [Header("Rotation")]
    [Tooltip("Maximum pitch up/down in degrees — keeps birds from facing straight down.")]
    public float maxPitchDegrees = 30f;

    [Header("Follow Origin")]
    public bool followOrigin = false;
    public float followOriginForce = 30.0f;
    public float followOriginRange = 60.0f;

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
        randomFreq      = Mathf.Max(0.0001f, randomFreq);
        toOriginRange   = Mathf.Max(0.0001f, toOriginRange);
        avoidanceRadius = Mathf.Max(0.0001f, avoidanceRadius);
        followRadius    = Mathf.Max(0.0001f, followRadius);

        randomFreqInterval = 1.0f / randomFreq;

        origin             = transform.parent;
        transformComponent = transform;

        UnityFlock[] tempFlocks = origin
            ? origin.GetComponentsInChildren<UnityFlock>()
            : new UnityFlock[] { this };

        objects     = new Transform[tempFlocks.Length];
        otherFlocks = new UnityFlock[tempFlocks.Length];
        for (int i = 0; i < tempFlocks.Length; i++)
        {
            objects[i]     = tempFlocks[i].transform;
            otherFlocks[i] = tempFlocks[i];
        }

        transform.parent   = null;
        velocity           = Random.onUnitSphere * minSpeed;
        normalizedVelocity = velocity.normalized;

        StartCoroutine(UpdateRandom());
    }

    IEnumerator UpdateRandom()
    {
        while (true)
        {
            randomPush = Random.insideUnitSphere * randomForce;
            yield return new WaitForSeconds(
                randomFreqInterval + Random.Range(-randomFreqInterval / 2.0f, randomFreqInterval / 2.0f));
        }
    }

    private void ApplyOriginFollow()
    {
        if (!followOrigin || origin == null) return;

        Vector3 toLeader = origin.position - transformComponent.position;
        float dist = toLeader.magnitude;

        float strength = Mathf.Clamp01(dist / followOriginRange) * followOriginForce;
        velocity += toLeader.normalized * strength;
    }

    // ── Obstacle avoidance ──────────────────────────────────────────────────
    private Vector3 ComputeObstacleAvoidance()
    {
        Vector3 avoid = Vector3.zero;
        Vector3 from  = transformComponent.position;
        Vector3 dir   = velocity.sqrMagnitude > 0.0001f ? velocity.normalized : transformComponent.forward;

        if (Physics.SphereCast(from, castRadius, dir, out RaycastHit hit, lookAhead, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 away = hit.normal;
            if (hit.normal.y > 0.3f) away += Vector3.up * 0.8f;

            float urgency = 1f - (hit.distance / lookAhead);
            avoid += away.normalized * obstacleAvoidForce * urgency;
        }

        if (Physics.Raycast(from, Vector3.down, out RaycastHit groundHit, minAltitude * 2f, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            if (groundHit.distance < minAltitude)
            {
                float urgency = 1f - (groundHit.distance / minAltitude);
                avoid += Vector3.up * altitudeForce * urgency;
            }
        }

        return avoid;
    }

    void Update()
    {
        Vector3 myPosition = transformComponent.position;

        Vector3 sumVelocity = Vector3.zero;
        Vector3 sumPosition = Vector3.zero;
        Vector3 separation  = Vector3.zero;
        int count = 0;

        for (int i = 0; i < objects.Length; i++)
        {
            Transform boidTransform = objects[i];
            if (boidTransform == null || boidTransform == transformComponent) continue;

            Vector3 otherPosition = boidTransform.position;
            sumPosition += otherPosition;
            sumVelocity += otherFlocks[i] != null ? otherFlocks[i].normalizedVelocity : Vector3.zero;

            Vector3 forceV = myPosition - otherPosition;
            float dist     = forceV.magnitude;

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
            avgVelocity = (sumVelocity / count) * followVelocity;
            toAvg       = (sumPosition / count) - myPosition;
            float followMag = Mathf.Clamp01(toAvg.magnitude / followRadius);
            avgVelocity *= followMag;
        }

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

        float speed = velocity.magnitude;
        if (speed > 0.000001f && speed < minSpeed)
            velocity = (velocity / speed) * minSpeed;

        Vector3 obstacleAvoid = ComputeObstacleAvoidance();

        Vector3 wantedVel = velocity;
        wantedVel -= wantedVel * Time.deltaTime;
        wantedVel += randomPush     * Time.deltaTime;
        wantedVel += originPush     * Time.deltaTime;
        wantedVel += avgVelocity    * Time.deltaTime;
        wantedVel += separation     * Time.deltaTime;
        wantedVel += obstacleAvoid  * Time.deltaTime;
        if (toAvg.sqrMagnitude > 0.000001f)
            wantedVel += gravity * Time.deltaTime * toAvg.normalized;

        velocity = Vector3.RotateTowards(velocity, wantedVel, turnSpeed * Time.deltaTime, 100.0f);

        if (!IsFinite(velocity))
            velocity = Random.onUnitSphere * minSpeed;

        // ── Rotation with pitch clamp + wings-level up vector ────────────────
        if (velocity.sqrMagnitude > 0.000001f)
        {
            Vector3 flatDir = new Vector3(velocity.x, 0f, velocity.z);
            if (flatDir.sqrMagnitude > 0.000001f)
            {
                flatDir.Normalize();

                float maxPitch     = maxPitchDegrees * Mathf.Deg2Rad;
                float currentPitch = Mathf.Atan2(velocity.y, new Vector2(velocity.x, velocity.z).magnitude);
                float clampedPitch = Mathf.Clamp(currentPitch, -maxPitch, maxPitch);

                Vector3 lookDir = flatDir * Mathf.Cos(clampedPitch) + Vector3.up * Mathf.Sin(clampedPitch);
                transformComponent.rotation = Quaternion.LookRotation(lookDir, Vector3.up);
            }
        }

        transformComponent.Translate(velocity * Time.deltaTime, Space.World);

        normalizedVelocity = (velocity.sqrMagnitude > 0.000001f) ? velocity.normalized : Vector3.zero;

        ApplyOriginFollow();
    }

    private static bool IsFinite(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
}