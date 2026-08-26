using UnityEngine;

namespace WormWars.Network
{
    // Purely local, non-networked follow chain for a worm's body segments. Since every
    // client already has an identical, synced head transform (via NetworkWormMovement's
    // NetworkVariables), each segment can independently chase the transform ahead of it
    // and the whole chain stays visually consistent across the network with zero extra
    // sync traffic.
    public class WormSegmentChain : MonoBehaviour
    {
        [SerializeField] Transform head;
        [SerializeField] Transform[] segments;

        [Header("Follow Tuning")]
        [SerializeField] float followSpeed = 10f;
        [SerializeField] float turnSpeed = 10f;
        [SerializeField] float segmentSpacing = 0.5f;

        void LateUpdate()
        {
            if (head == null || segments == null || segments.Length == 0) return;

            Transform previous = head;
            for (int i = 0; i < segments.Length; i++)
            {
                Transform segment = segments[i];
                if (segment == null) { previous = segment; continue; }

                Vector3 targetPosition = previous.position - previous.forward * segmentSpacing;
                segment.position = Vector3.Lerp(segment.position, targetPosition, followSpeed * Time.deltaTime);
                segment.rotation = Quaternion.Slerp(segment.rotation, previous.rotation, turnSpeed * Time.deltaTime);

                previous = segment;
            }
        }

        // Places every segment directly behind the head with no lerp, so a fresh spawn
        // (or a remote client just joining) doesn't show the chain sliding into place.
        public void SnapToHead()
        {
            if (head == null || segments == null) return;

            Transform previous = head;
            for (int i = 0; i < segments.Length; i++)
            {
                Transform segment = segments[i];
                if (segment == null) { previous = segment; continue; }

                segment.SetPositionAndRotation(previous.position - previous.forward * segmentSpacing, previous.rotation);
                previous = segment;
            }
        }
    }
}
