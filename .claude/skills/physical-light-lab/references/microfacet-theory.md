# Microfacet shading: why surfaces look the way they do

A real surface is never perfectly smooth. Zoom in past what a camera pixel can
resolve and you find a landscape of tiny mirror-like facets, each pointing in
a slightly different direction. Microfacet theory says: don't try to model
every facet — model their *statistics*. That single idea explains why metal,
plastic, and brushed foil all reflect light so differently, and it's exactly
what `THREE.MeshPhysicalMaterial` computes per pixel on the GPU.

The Cook-Torrance BRDF (bidirectional reflectance distribution function)
splits the specular response into three terms:

```
specular = (D * F * G) / (4 * (N·V) * (N·L))
```

- **N** = surface normal, **V** = direction to viewer, **L** = direction to light,
  **H** = the halfway vector between V and L (the orientation a facet needs to
  bounce light from L straight into V).

## D — the normal distribution (roughness)

D answers: *what fraction of facets are angled just right to reflect the
light into the camera?* The GGX/Trowbridge-Reitz distribution is the
industry-standard choice because its "long tail" matches real materials —
it produces a bright core with a soft falloff instead of an artificial hard
edge.

- **Low roughness** → facet normals are tightly clustered around the true
  surface normal → only a narrow cone of view/light angles catches a facet
  aimed correctly → a small, sharp, intense highlight (freshly polished
  metal, still water).
- **High roughness** → facet normals scatter widely → light exits in many
  directions no matter where the camera is → a broad, dim, soft highlight
  (chalk, brushed aluminum, matte paint).

This is `material.roughness` in three.js (0 = mirror, 1 = fully diffuse-looking
specular). **Roughness is the single biggest visual lever** — it's worth
exposing as the first slider in any light-study build.

## F — Fresnel reflectance (why edges shine)

Every surface, even "non-reflective" plastic, becomes a near-perfect mirror
at a glancing angle. Stand water bottles or windows nearly edge-on to your
eye and you'll see it: the reflection intensifies as the viewing angle gets
more oblique. This is the Fresnel effect, and the Fresnel-Schlick
approximation makes it cheap to compute:

```
F(θ) = F0 + (1 - F0) * (1 - cos θ)^5
```

`θ` is the angle between V and H. `F0` is the reflectance straight-on
(looking directly at the surface) — it's low (~0.02–0.05, a pale gray) for
dielectrics like plastic or paint, and it's *tinted by the material's color*
for metals (gold's F0 is warm yellow, copper's is orange-red). This is why
metals need a colored specular highlight and dielectrics get a white one —
and it's exactly the distinction `material.metalness` encodes in three.js:
it interpolates F0 between "always ~4% white" (metalness=0) and "the
albedo color itself" (metalness=1).

Grazing-angle brightening from the `(1 - cos θ)^5` term is why rim
lighting, car-paint edge-glow, and the bright halo around spheres lit from
behind all look the way they do — it is a real optical law, not a rendering
trick.

## G — geometric shadowing/masking (why rough surfaces darken at grazing angles)

At a glancing angle, tall microfacets on a rough surface start blocking
their neighbors — some facets shadow the light source from reaching a
neighboring facet ("shadowing"), and some block the reflected ray from
reaching the eye ("masking"). The Smith geometry term statistically accounts
for this self-occlusion. Without it, rough materials would look
unrealistically bright at grazing angles because D and F alone assume every
facet is fully visible and fully lit.

The net visible effect: **rough surfaces get relatively dimmer, and smooth
surfaces stay relatively brighter, as you rotate toward a grazing view.**
This is subtle compared to D and F, so it rarely needs its own slider, but
it's worth explaining in prose — it's the piece that keeps roughness/Fresnel
interactions from looking wrong at silhouette edges.

## Mapping to three.js `MeshPhysicalMaterial`

| Physical concept | Property | Typical range |
|---|---|---|
| D term width | `roughness` | 0.05 (polished) – 0.9 (chalky) |
| F0 tint / metal vs. dielectric | `metalness` | 0 (plastic/paint) or 1 (bare metal) |
| Overall reflectance color | `color` | the albedo/base color |
| Extra mirror coat over a base layer (car paint, lacquer, anodized coating) | `clearcoat`, `clearcoatRoughness` | 0–1 |

G is handled internally — there's no slider for it, but note in your UI copy
that it's why the highlight doesn't blow out at the object's silhouette.

**Building intuition for a user:** expose `roughness` and `metalness` as
two sliders and narrate the four corners — (low roughness, metalness 0) is
wet plastic; (low roughness, metalness 1) is a mirror-polished metal;
(high roughness, metalness 0) is chalk/paper; (high roughness, metalness 1)
is brushed/oxidized metal (anodized aluminum, cast iron). Letting someone
drag between those corners *is* the lesson.
