using UnityEngine;

namespace WormWars.Network
{
    // Draws the local player's aiming preview as a dotted line of instanced dots, entirely
    // outside of LineRenderer/URP/HDRP. Reads its points straight from a
    // NetworkTrajectoryPredictor and pushes them to the GPU in a single
    // Graphics.DrawMeshInstanced call so a custom render pipeline never has to touch a
    // per-dot GameObject.
    public class CustomPipelineTrajectoryRenderer : MonoBehaviour
    {
        // Hard cap imposed by Graphics.DrawMeshInstanced itself.
        const int MaxInstancesPerDrawCall = 1023;

        [Header("Data Source")]
        [SerializeField] NetworkTrajectoryPredictor trajectoryPredictor;

        [Header("Dot Visuals")]
        [SerializeField] Mesh dotMesh;
        [SerializeField] Material dotMaterial;
        [SerializeField] float dotScale = 0.25f;
        [SerializeField, Range(1, MaxInstancesPerDrawCall)] int maxDots = 30;
        [SerializeField] Color dotColor = Color.white;

        static readonly int ColorPropertyId = Shader.PropertyToID("_Color");

        // Allocated once at the configured capacity and reused every frame - never resized -
        // so drawing the preview generates zero per-frame GC garbage.
        Matrix4x4[] _matrices;
        MaterialPropertyBlock _propertyBlock;

        void Awake()
        {
            _matrices = new Matrix4x4[Mathf.Clamp(maxDots, 1, MaxInstancesPerDrawCall)];
            _propertyBlock = new MaterialPropertyBlock();
            if (dotMesh == null) dotMesh = BuildDefaultDotMesh();
        }

        void LateUpdate()
        {
            if (!ShouldDraw(out Vector3[] points)) return;

            int count = Mathf.Min(points.Length, _matrices.Length);
            if (count <= 0) return;

            for (int i = 0; i < count; i++)
            {
                _matrices[i] = Matrix4x4.TRS(points[i], Quaternion.identity, Vector3.one * dotScale);
            }

            _propertyBlock.SetColor(ColorPropertyId, dotColor);
            Graphics.DrawMeshInstanced(dotMesh, 0, dotMaterial, _matrices, count, _propertyBlock);
        }

        bool ShouldDraw(out Vector3[] points)
        {
            points = null;
            if (trajectoryPredictor == null || dotMesh == null || dotMaterial == null) return false;

            // Only the local, currently-aiming owner should see (and pay the cost of) a preview.
            if (!trajectoryPredictor.IsOwner || !trajectoryPredictor.IsAiming) return false;

            points = trajectoryPredictor.PreviewPoints;
            return points != null;
        }

        // Minimal built-in fallback so the renderer works before a custom mesh is assigned.
        // A caller wanting the dots to read cleanly from every camera angle should supply
        // their own sphere mesh instead - this quad isn't billboarded.
        static Mesh BuildDefaultDotMesh()
        {
            var mesh = new Mesh { name = "TrajectoryDotQuad" };
            mesh.vertices = new[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2, 0, 1, 2, 0, 2, 3 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}
