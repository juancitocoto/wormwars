using UnityEngine;

namespace WormWars.Core
{
    // 3D counterpart to ProceduralSprite: spawns tinted primitive geometry at runtime so
    // procedural builds (see Assets/Scripts/World/Castle3DView.cs) never depend on imported
    // meshes, textures, or materials. Replace call sites with authored art as it lands;
    // nothing else depends on this staying around.
    public static class ProceduralPrimitive3D
    {
        public static Transform Block(string name, Transform parent, Vector3 size, Vector3 localPosition, Color color, Quaternion? localRotation = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            var t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            t.localRotation = localRotation ?? Quaternion.identity;
            t.localScale = size;
            Tint(go, color);
            return t;
        }

        public static Transform Cylinder(string name, Transform parent, float radius, float height, Vector3 localPosition, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            var t = go.transform;
            t.SetParent(parent, false);
            t.localPosition = localPosition;
            // Unity's built-in cylinder primitive is 2 units tall with a 1-unit diameter.
            t.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            Tint(go, color);
            return t;
        }

        // .material (not .sharedMaterial) instantiates a per-renderer copy automatically, so
        // recoloring one block never bleeds into every other primitive built this way.
        public static void Tint(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = color;
        }
    }
}
