# Build patterns for a self-contained light-study artifact

These renders are almost always delivered as a single HTML file (a Claude
Artifact or a standalone file the user opens locally). Artifacts run under a
strict CSP: no CDN `<script src>`, no external fetches. Everything must be
inlined.

## 1. Inline three.js instead of linking it

Read `assets/vendor/three.min.js` (bundled with this skill, three.js r160,
UMD build — attaches `window.THREE`) and paste its full contents inside a
`<script>` tag near the top of the page. Same for
`assets/orbit-controls.js`. Do not try to `fetch()` or CDN-link either one —
both the Artifact sandbox and a locally-opened `file://` HTML page will
refuse cross-origin script loads.

The vendor file prints one harmless `console.warn` about the UMD build being
deprecated in favor of ES modules — ignore it, it doesn't affect
functionality, and there's no CDN-free way to use the ES module build
without a bundler.

```html
<script>
/* ... full contents of assets/vendor/three.min.js, pasted verbatim ... */
</script>
<script>
/* ... full contents of assets/orbit-controls.js, pasted verbatim ... */
</script>
<script>
  // your scene code — THREE and createOrbitControls are now globals
</script>
```

## 2. Scene skeleton

```js
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x05070c); // dark backdrop reads best for specular studies

const camera = new THREE.PerspectiveCamera(45, innerWidth / innerHeight, 0.1, 100);
camera.position.set(4, 2.5, 6);

const renderer = new THREE.WebGLRenderer({ antialias: true, canvas: document.getElementById('c') });
renderer.setPixelRatio(Math.min(devicePixelRatio, 2));
renderer.setSize(innerWidth, innerHeight);
renderer.outputColorSpace = THREE.SRGBColorSpace;
renderer.toneMapping = THREE.ACESFilmicToneMapping; // needed for physical materials to look correct, not blown out

const controls = createOrbitControls(camera, renderer.domElement, { target: new THREE.Vector3(0, 0, 0) });

// Lighting: a key light + a dim ambient/hemisphere fill. A single bright point
// or directional light is important for a light-study — it gives roughness
// and Fresnel effects a clear, legible highlight to move as the camera orbits.
const key = new THREE.DirectionalLight(0xffffff, 3.0);
key.position.set(5, 6, 4);
scene.add(key);
scene.add(new THREE.HemisphereLight(0x8899aa, 0x111214, 0.6));

// An environment map matters more than people expect: MeshPhysicalMaterial's
// specular/iridescent response looks flat and "video-gamey" without one,
// because there's nothing but the single point light to reflect. A cheap
// synthetic environment (a few colored panels in a PMREM-generated room) reads
// far better than no environment at all:
const pmrem = new THREE.PMREMGenerator(renderer);
const envScene = new THREE.Scene();
envScene.add(new THREE.Mesh(new THREE.SphereGeometry(20, 16, 16),
  new THREE.MeshBasicMaterial({ color: 0x223344, side: THREE.BackSide })));
const panel = new THREE.Mesh(new THREE.PlaneGeometry(6, 6), new THREE.MeshBasicMaterial({ color: 0xffffff }));
panel.position.set(0, 8, -6); envScene.add(panel);
scene.environment = pmrem.fromScene(envScene, 0.04).texture;

function animate() {
  requestAnimationFrame(animate);
  controls.update();
  renderer.render(scene, camera);
}
animate();

addEventListener('resize', () => {
  camera.aspect = innerWidth / innerHeight;
  camera.updateProjectionMatrix();
  renderer.setSize(innerWidth, innerHeight);
});
```

## 3. Building a probe (or any object) from primitives

Don't reach for an external model file — none can be loaded under the CSP
anyway. Assemble recognizable shapes from primitives and group them; a space
probe reads immediately from just:

- a `CylinderGeometry` or `IcosahedronGeometry` bus (main body),
- a shallow `CylinderGeometry` (radial segments high, top/bottom radius
  different) or a custom lathe for the parabolic dish,
- thin `BoxGeometry` panels for solar arrays, angled outward,
- a few `CylinderGeometry` struts/antennas.

Assign a **different material per part** on purpose — this is what lets one
object teach several material stories at once (see the material table
below). Use `THREE.Group` to compose them and to give yourself one object to
hand to the orbit-ray-visualization code.

## 4. Wiring sliders directly to material properties

Skip a UI framework — a light-study artifact has few enough controls that
plain DOM + `oninput` is clearer to read and modify than any component
system:

```html
<div class="panel">
  <label>Roughness <span id="roughnessVal"></span></label>
  <input id="roughness" type="range" min="0.02" max="0.95" step="0.01" value="0.35">
  <p class="explain" id="roughnessExplain"></p>
</div>
```

```js
const roughnessSlider = document.getElementById('roughness');
roughnessSlider.addEventListener('input', () => {
  const v = parseFloat(roughnessSlider.value);
  bodyMaterial.roughness = v;
  document.getElementById('roughnessVal').textContent = v.toFixed(2);
  document.getElementById('roughnessExplain').textContent =
    v < 0.15
      ? 'Facet normals are tightly clustered: a small, sharp, intense highlight.'
      : v > 0.6
      ? 'Facet normals scatter widely: light exits in many directions, so the highlight is broad and soft.'
      : 'A middle ground — highlight is visible but has noticeable spread.';
});
roughnessSlider.dispatchEvent(new Event('input')); // sync UI + explain text on load
```

The pattern to repeat for every slider: **the explanation text must update
live with the value**, not sit static in a caption — that's what turns a
slider into a lesson instead of a toy.

## 5. Suggested material table for a probe

| Part | Material recipe | Teaches |
|---|---|---|
| Main bus / MLI wrap | `MeshPhysicalMaterial` with `iridescence` on, gold/amber base color | thin-film interference |
| Solar panels | low `roughness`, dark blue-black base color, slight `metalness` | sharp specular highlight, grid-like micro-reflections |
| Parabolic dish | `metalness: 1`, `roughness` exposed to the main slider, white/silver base | the primary microfacet (roughness) demo |
| Structural booms/struts | `metalness: 1`, fixed mid roughness, unlit-ish | context geometry, not a teaching surface — keep it out of the way of the sliders |

## 6. Ray-transport overlay

Build this as a `THREE.Group` of `LineSegments` recomputed whenever the
object rotates or a relevant slider (roughness) changes — see
`references/ray-transport.md` for the geometry math. Toggle its visibility
with a checkbox rather than always showing it; it's a diagram layered on top
of the render, not part of the render itself.

## 7. Theme-aware UI chrome

The 3D canvas is inherently a dark viewport (see the scene skeleton above)
and should generally stay that way regardless of the artifact's light/dark
theme — a light-study reads best against a near-black backdrop, the same way
a photo studio does. But the surrounding UI chrome (slider panel, text,
background around the canvas) must still follow the standard Artifact
light/dark theme rules from the `artifact-design` skill: define tokens on
`:root`, override under `prefers-color-scheme: dark` and
`[data-theme="dark"]`, and never hardcode a light-only panel background.
