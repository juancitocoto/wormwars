using UnityEngine;

namespace WormWars.Core
{
    // World-space dashed arc, visible only while AimJoystickUI is dragging (or the fire
    // button is charging). Dashes "march" via material texture offset scrolling instead of
    // an animated point list, matching the spec's 800ms-loop marching-dash animation.
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryArc : MonoBehaviour
    {
        public int pointCount = 24;
        public float gravity = 20f; // dp/s^2, tuned for on-screen readability rather than realism
        public float marchSpeed = 1.25f; // texture offset units/sec

        LineRenderer _line;
        Material _material;

        void Awake()
        {
            _line = GetComponent<LineRenderer>();
            _line.positionCount = 0;
            _line.widthMultiplier = 2f;
            _line.textureMode = LineTextureMode.Tile;

            _material = new Material(Shader.Find("Sprites/Default"));
            _line.material = _material;
            SetVisible(false);
        }

        void Update()
        {
            if (!_line.enabled) return;
            float offset = (Time.time * marchSpeed) % 1f;
            _material.mainTextureOffset = new Vector2(offset, 0f);
        }

        public void SetVisible(bool visible) => _line.enabled = visible;

        public void Draw(Vector3 origin, float angleDeg, float launchSpeed, Vector2 windAccel)
        {
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * launchSpeed;
            Vector2 accel = new Vector2(windAccel.x, windAccel.y - gravity);

            var points = new Vector3[pointCount];
            float dt = 0.08f;
            Vector2 pos = Vector2.zero;
            Vector2 vel = velocity;

            for (int i = 0; i < pointCount; i++)
            {
                points[i] = origin + (Vector3)pos;
                vel += accel * dt;
                pos += vel * dt;
            }

            _line.positionCount = pointCount;
            _line.SetPositions(points);
        }
    }
}
