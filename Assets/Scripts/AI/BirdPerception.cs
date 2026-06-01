using UnityEngine;

namespace BirdAI
{
    /// Minimal raycast helper. Given a target position, returns whether
    /// the bird has clear line-of-sight to it (uses cover layers as blockers).
    ///
    /// All decision logic — distance, FOV, who counts as "the player",
    /// memory of last-known-position — lives in the behavior tree.
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
            Vector3 from = eye.position;
            Vector3 dir  = target - from;
            float dist   = dir.magnitude;

            if (dist < 0.01f) return true;

            dir /= dist;

            Vector3 origin     = from + dir * 0.3f;
            float adjustedDist = dist - 0.3f;
            if (adjustedDist <= 0f) return true;

            bool blocked = Physics.Raycast(
                origin, dir, out RaycastHit hit,
                adjustedDist, coverMask,
                QueryTriggerInteraction.Ignore);

            Debug.DrawRay(origin, dir * adjustedDist, blocked ? Color.red : Color.green, 0.1f);
            if (blocked)
                Debug.Log($"[Perception] Blocked by {hit.collider.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)}");

            return !blocked;
        }
    }
}
