using UnityEngine;

namespace WormWars.Core
{
    // Placeholder world-space visual for a worm. Per the spec's accessibility note, team
    // identity must never rely on color alone, so each team also gets a distinct marker
    // shape (Team A: triangle, Team B: square) in addition to its color.
    [RequireComponent(typeof(WormController))]
    public class WormView : MonoBehaviour
    {
        public float bodyDiameterDp = 28f;
        public float hpPillWidthDp = 32f;
        public float hpPillHeightDp = 6f;
        public float hpPillOffsetDp = 14f;

        WormController _worm;
        SpriteRenderer _body;
        SpriteRenderer _marker;
        SpriteRenderer _star;
        Transform _hpPillRoot;
        SpriteRenderer _hpTrack;
        SpriteRenderer _hpFill;

        float _bobTimer;

        void Awake()
        {
            _worm = GetComponent<WormController>();

            _body = CreateLayer("Body", 1);
            _body.sprite = ProceduralSprite.RoundedRect(Mathf.RoundToInt(bodyDiameterDp), Mathf.RoundToInt(bodyDiameterDp), Mathf.RoundToInt(bodyDiameterDp / 2f), Color.white);
            _body.color = DesignTokens.Color_.ForTeam(_worm.teamId);

            _marker = CreateLayer("Marker", 2);
            int markerSize = Mathf.RoundToInt(bodyDiameterDp * 0.4f);
            _marker.sprite = _worm.teamId == TeamId.A
                ? ProceduralSprite.RoundedRect(markerSize, markerSize, 2, Color.white)
                : ProceduralSprite.RoundedRect(markerSize, markerSize, 0, Color.white);
            _marker.color = DesignTokens.Color_.Cream;
            _marker.transform.localRotation = _worm.teamId == TeamId.A ? Quaternion.Euler(0, 0, 45) : Quaternion.identity;

            _star = CreateLayer("ActiveMarker", 3);
            _star.sprite = ProceduralSprite.RoundedRect(6, 6, 3, Color.white);
            _star.color = Color.yellow;
            _star.transform.localPosition = new Vector3(0, bodyDiameterDp * 0.5f + 8f, 0);
            _star.enabled = false;

            BuildHpPill();

            _worm.OnStateChanged += _ => Refresh();
            _worm.OnHit += _ => RefreshHp();
            _worm.OnEliminated += _ => gameObject.SetActive(false);
            Refresh();
            RefreshHp();
        }

        void BuildHpPill()
        {
            var root = new GameObject("HpPill");
            root.transform.SetParent(transform, false);
            root.transform.localPosition = new Vector3(0, bodyDiameterDp * 0.5f + hpPillOffsetDp, 0);
            _hpPillRoot = root.transform;

            _hpTrack = CreateChildLayer(root.transform, "Track", 4, hpPillWidthDp, hpPillHeightDp, DesignTokens.Color_.HpTrack);
            _hpFill = CreateChildLayer(root.transform, "Fill", 5, hpPillWidthDp, hpPillHeightDp, DesignTokens.Color_.HpFillStart);
        }

        SpriteRenderer CreateChildLayer(Transform parent, string name, int order, float w, float h, Color color)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            sr.sprite = ProceduralSprite.RoundedRect(Mathf.RoundToInt(w), Mathf.RoundToInt(h), Mathf.RoundToInt(h / 2f), Color.white);
            sr.color = color;
            return sr;
        }

        SpriteRenderer CreateLayer(string name, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = order;
            return sr;
        }

        void Update()
        {
            if (_worm.VisualState != WormVisualState.Active) return;
            _bobTimer += Time.deltaTime;
            float bob = Mathf.Sin(_bobTimer / 2f * Mathf.PI * 2f) * 2f;
            _body.transform.localPosition = new Vector3(0, bob, 0);
        }

        void Refresh()
        {
            _star.enabled = _worm.VisualState == WormVisualState.Active || _worm.VisualState == WormVisualState.Aiming;
            if (_worm.VisualState != WormVisualState.Active) _body.transform.localPosition = Vector3.zero;
        }

        void RefreshHp()
        {
            float pct = Mathf.Max(0.02f, _worm.HPPercent01);
            _hpFill.transform.localScale = new Vector3(pct, 1f, 1f);
            _hpFill.transform.localPosition = new Vector3(-hpPillWidthDp * (1f - pct) * 0.5f, 0f, -0.01f);
            _hpFill.color = _worm.HPPercent01 <= 0.25f ? DesignTokens.Color_.Fire : DesignTokens.Color_.HpFillStart;
        }
    }
}
