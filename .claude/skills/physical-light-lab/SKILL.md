---
name: physical-light-lab
description: Build interactive, physically-based 3D renders that teach how light actually behaves on an object's surface at the microscopic material level — microfacet/Cook-Torrance shading (roughness, metalness, Fresnel-Schlick reflectance, GGX highlight spread), thin-film iridescence (the rainbow shimmer on foil, oil slicks, spacecraft insulation blankets), and ray-level light transport (how photons bounce from source to surface to eye). Use this whenever the user wants to render an object — a spacecraft/probe, a piece of jewelry, a car, any physical thing — AND understand or explain *why* it looks shiny, matte, iridescent, or metallic, not just see a picture of it. Trigger on requests to "study light," "visualize PBR/reflectance," "show how light interacts with [material/surface]," "explain Fresnel/roughness/GGX/thin-film," "microscopic light behavior," or "build a light lab / material study," even if the user doesn't name the physics terms themselves — "why does this look so shiny" or "make the gold foil shimmer like a real satellite" are the skill's home turf.
---

# Physical Light Lab

The goal of this skill is never just a pretty render. It's an interactive
artifact where turning a slider or rotating the object *demonstrates a real
physical law*, with plain-language text that explains what's happening and
why, live, as the control moves. If the deliverable doesn't teach something
when you touch it, it's a picture, not a light lab.

## Workflow

1. **Pin down which phenomena the user actually wants to explore.** The
   three building blocks are independent and composable:
   - *Microfacet shading* (roughness/metalness/Fresnel) — always include
     this one; it's the foundation everything else sits on. See
     `references/microfacet-theory.md`.
   - *Thin-film iridescence* — include when the object has a foil, coating,
     or oxide-layer surface (spacecraft MLI blankets, soap film, oil,
     anodized metal, beetle shells). See
     `references/thin-film-interference.md`.
   - *Ray-level transport* — include when the user wants to see the
     mechanics of bouncing/reflection itself, not just the resulting shading.
     See `references/ray-transport.md`.
   If the user asks for "a light study" without specifying, default to all
   three — that combination is what makes a single object teach a complete
   story (see the probe example below), and it's cheap to add given three.js's
   materials already implement the first two.

2. **Choose or build the object as a small set of primitives**, each part
   assigned a *deliberately different* material so the render teaches
   several things at once instead of just looking like one shiny blob. Don't
   look for an external 3D model file — none can load under the Artifact
   CSP, and primitives assembled with intent (see
   `references/threejs-patterns.md` §3, §5) read perfectly well and stay
   fast. For a space probe specifically: a cylindrical/faceted bus (gets the
   iridescent foil), a parabolic dish (gets the main roughness slider), flat
   angled solar panels (sharp low-roughness highlights), and a couple of
   struts/antennas for silhouette.

3. **Build the scene as one self-contained HTML file.** Read
   `references/threejs-patterns.md` for the concrete wiring: how to inline
   `assets/vendor/three.min.js` and `assets/orbit-controls.js` (both bundled
   with this skill — read their file contents and paste them into `<script>`
   tags; nothing may be loaded from a CDN), the lighting/environment setup
   physical materials need to look correct, and the animation loop.

4. **Expose the physics as live-updating sliders/toggles, never static
   values.** At minimum: a roughness slider on the main teaching surface,
   a metalness toggle or slider, and — if iridescence is in scope — an
   iridescence strength and thickness-range slider. Each control's
   explanation text must recompute on `input`, not just its numeric readout
   — see the wiring pattern in `references/threejs-patterns.md` §4. This is
   the difference between a demo and a lesson.

5. **If ray transport is in scope, add it as a toggleable overlay**, not a
   permanent part of the render — it's a diagram of what the shader is doing
   analytically, drawn on top. Tie its visual spread to the roughness slider
   so the two references reinforce each other (turning roughness up should
   visibly widen the ray fan at the same time the highlight broadens).

6. **Write the explanatory copy to answer "why," not just "what."** E.g. not
   "Roughness: 0.4" but "Facet normals are scattering across a moderate
   range, so the highlight is visible but no longer sharp — this is why
   brushed metal looks different from a mirror even though both are the same
   base metal." Pull the actual mechanism from the relevant reference file
   rather than paraphrasing loosely; the references contain the real
   equations and the correct plain-language mapping to each three.js
   property, and getting the physics right is the entire point of this
   skill.

7. **Respect Artifact constraints and design quality.** Before publishing,
   load the `artifact-design` skill for layout/typography fundamentals and
   follow the theme-awareness note in `references/threejs-patterns.md` §7
   for the UI chrome (the 3D viewport itself should stay a dark "studio"
   background regardless of theme). Publish with the `Artifact` tool.

8. **Sanity-check the physics before calling it done:**
   - Rotate the object with iridescence on — does the color actually shift
     with viewing angle, or is it a static gradient? (If static, iridescence
     isn't wired to the camera/normal-dependent thickness — re-check
     `iridescenceThicknessRange` is set, not just `iridescence`.)
   - Drag roughness from low to high — does the highlight visibly broaden
     and dim, not just change color?
   - At a grazing/silhouette angle, does a low-roughness metal surface
     brighten noticeably? (Fresnel.) If nothing changes at grazing angles,
     the environment map is probably missing — a bare point light alone
     under-demonstrates Fresnel because there's little to reflect at glancing
     angles. Add the synthetic environment from
     `references/threejs-patterns.md` §2.

## Reference index

- `references/microfacet-theory.md` — Fresnel-Schlick, GGX distribution,
  Smith geometry term, and the roughness/metalness mapping.
- `references/thin-film-interference.md` — thin-film wave interference and
  the iridescence/iridescenceIOR/iridescenceThicknessRange mapping.
- `references/ray-transport.md` — what a light-transport path is, why path
  tracing converges with more samples, and how to draw a ray-bounce overlay.
- `references/threejs-patterns.md` — the concrete build: inlining the
  bundled three.js, scene/lighting/environment skeleton, primitive-based
  object construction, slider wiring, and theme-aware chrome.

## Bundled assets

- `assets/vendor/three.min.js` — three.js r160, UMD build (sets
  `window.THREE`). Inline its full contents; don't fetch or CDN-link it.
- `assets/orbit-controls.js` — a small dependency-free drag/wheel/touch
  orbit camera control, since three.js's official `OrbitControls` ships as
  an ES module that assumes a bundler. Inline this too.
