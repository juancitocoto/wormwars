using UnityEngine;

namespace WormWars.Core
{
    // Placeholder world-space visual for a castle: a stone-colored rounded rect with a
    // cream "interior" cutaway rect, driven by CastleController's derived damage stage.
    // Swap the sprite children for real dollhouse-cutaway art without touching CastleController.
    [RequireComponent(typeof(CastleController))]
    public class CastleView : MonoBehaviour
    {
        public float widthDp = 200f;
        public float heightDp = 160f;
        public bool interiorFacesRight = true;

        CastleController _castle;
        SpriteRenderer _wall;
        SpriteRenderer _interior;
        SpriteRenderer _smokeOverlay;
        bool _initialized;

        // Called explicitly by BattleLayoutBuilder after widthDp/heightDp/interiorFacesRight
        // are set — AddComponent() runs Awake() synchronously on an active GameObject, which
        // would be before the builder gets a chance to assign those fields.
        public void Init()
        {
            if (_initialized) return;
            _initialized = true;

            _castle = GetComponent<CastleController>();

            _wall = CreateLayer("Wall", 0);
            _wall.sprite = ProceduralSprite.RoundedRect(Mathf.RoundToInt(widthDp), Mathf.RoundToInt(heightDp), 14, Color.white);
            _wall.color = DesignTokens.Color_.Stone;

            float interiorW = widthDp * 0.62f;
            float interiorH = heightDp * 0.72f;
            _interior = CreateLayer("Interior", 1);
            _interior.sprite = ProceduralSprite.RoundedRect(Mathf.RoundToInt(interiorW), Mathf.RoundToInt(interiorH), 8, Color.white);
            _interior.color = DesignTokens.Color_.Interior;
            float xOffset = (widthDp - interiorW) * 0.5f * (interiorFacesRight ? 1f : -1f) * 0.5f;
            _interior.transform.localPosition = new Vector3(xOffset, 0f, -0.01f);

            _smokeOverlay = CreateLayer("SmokeOverlay", 2);
            _smokeOverlay.sprite = _wall.sprite;
            _smokeOverlay.color = new Color(0.15f, 0.13f, 0.12f, 0f);

            _castle.OnDamaged += _ => Refresh();
            _castle.OnStageChanged += _ => Refresh();
            Refresh();
        }

        SpriteRenderer CreateLayer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        void Refresh()
        {
            switch (_castle.Stage)
            {
                case CastleDamageStage.Intact:
                    _smokeOverlay.color = new Color(0.15f, 0.13f, 0.12f, 0f);
                    _wall.color = DesignTokens.Color_.Stone;
                    break;
                case CastleDamageStage.Shockwave:
                    _smokeOverlay.color = new Color(0.15f, 0.13f, 0.12f, 0.08f);
                    break;
                case CastleDamageStage.Smoking:
                    _smokeOverlay.color = new Color(0.15f, 0.13f, 0.12f, 0.20f);
                    break;
                case CastleDamageStage.Rubble:
                    _smokeOverlay.color = new Color(0.15f, 0.13f, 0.12f, 0.35f);
                    _wall.color = DesignTokens.Color_.StoneDark;
                    break;
                case CastleDamageStage.Breached:
                    _smokeOverlay.color = new Color(0.1f, 0.08f, 0.08f, 0.55f);
                    _wall.color = DesignTokens.Color_.StoneDark;
                    break;
                case CastleDamageStage.Destroyed:
                    _wall.enabled = false;
                    _interior.enabled = false;
                    _smokeOverlay.color = new Color(0.1f, 0.08f, 0.08f, 0.75f);
                    break;
            }
        }
    }
}
