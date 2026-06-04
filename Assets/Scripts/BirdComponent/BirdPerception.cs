using UnityEngine;

namespace BirdAI
{
    // Minimal raycast helper. Given a target position, returns whether
    // the bird has clear line-of-sight to it (uses mask layers as blockers).
    
    // All decision logic — distance, FOV, who counts as "the player",
    // memory of last-known-position — lives in the behavior tree. (rally point)
    public class BirdPerception : MonoBehaviour
    {
        [Header("Vision")]
        [Tooltip("Layers whose colliders block line-of-sight (buildings, rocks, trees, doors).")]
        public LayerMask coverMask = ~0;

        [Tooltip("Empty child at the bird's eye position. Defaults to the bird transform.")]
        public Transform eye;

        void Start()
        {
            if (eye == null) eye = transform;
        }

        /// True if a raycast from the bird's eye to `target` isn't blocked by cover.
        public bool CanSeeTarget(Vector3 target)
        {
            Vector3 eyePosition = eye.position;
            Vector3 directionToTarget = target - eyePosition;
            float distanceToTarget = directionToTarget.magnitude;

            if (distanceToTarget < 0.01f) return true;

            directionToTarget /= distanceToTarget;

            // Start the ray slightly in front of the eye to avoid self-hits
            float skinOffset = 0.3f;
            Vector3 rayOrigin = eyePosition + directionToTarget * skinOffset;
            float rayDistance = distanceToTarget - skinOffset;
            if (rayDistance <= 0f) return true;

            bool blocked = Physics.Raycast(
                rayOrigin, directionToTarget, out RaycastHit hitInfo,
                rayDistance, coverMask,
                QueryTriggerInteraction.Ignore);

            Debug.DrawRay(rayOrigin, directionToTarget * rayDistance, blocked ? Color.red : Color.green, 0.1f);
            if (blocked)
                Debug.Log($"[Perception] Blocked by {hitInfo.collider.name} on layer {LayerMask.LayerToName(hitInfo.collider.gameObject.layer)}");

            return !blocked;
        }
    }
}
