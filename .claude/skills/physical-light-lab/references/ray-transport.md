# Ray-level light transport: making photon paths visible

Microfacet theory and thin-film interference both describe *what happens at
one surface point*. Ray transport is the other half of the picture: *how
light gets from the source to that point, and from that point to the eye,
at all* — the geometry of bouncing, not the material response at the bounce.

## The core idea

A physically-based renderer conceptually traces light transport paths:
light leaves a source, travels in a straight line until it hits a surface,
and at that surface it either:

- **reflects** in (or near) the mirror direction — governed by the D and F
  terms from microfacet theory (a rough surface "reflects" into a spread of
  directions, not one),
- gets **absorbed** (turned into heat, not light — this is what makes dark
  materials dark), or
- **transmits/refracts** through, if the material is transparent.

A path tracer works backward from the camera: for each pixel, shoot a ray
into the scene, bounce it off whatever it hits (choosing a bounce direction
statistically, weighted by the surface's BRDF), and keep bouncing until the
ray reaches a light source or a maximum bounce count. Average many random
paths per pixel and the noisy individual samples converge to a smooth image
— this is why raising "samples per pixel" in any path tracer reduces grain
at the cost of render time; it's literally averaging more randomly-sampled
light paths.

Real-time engines like three.js's default renderer don't path-trace live —
they approximate the *direct* lighting term (source → surface → eye,
one bounce) analytically using the BRDF math from microfacet-theory.md, which
is exactly what makes real-time PBR possible. But the underlying physical
picture — rays leaving a source, bouncing according to the surface's
statistics, arriving at the eye — is identical. It's a great thing to render
explicitly as an overlay, even in a real-time scene, because it's the part
of the picture that's normally invisible.

## Visualizing it in three.js

You don't need a path tracer to draw the *idea* of ray transport. Sample a
handful of points on the object's visible surface, and for each one:

1. Compute the incoming light direction `L` (from the point to your light
   source) and the surface normal `N` at that point.
2. Compute the ideal mirror bounce direction with `L.clone().negate().reflect(N)`
   (three.js's `Vector3.reflect` does the reflection-law math for you —
   `R = L - 2(L·N)N`).
3. Draw a short line segment from the light, to the surface point, and along
   the reflected direction — a `THREE.BufferGeometry` fed to
   `THREE.LineSegments` with a bright, unlit `LineBasicMaterial` reads
   clearly against the shaded object.
4. For a rough surface, draw *several* reflected rays per point fanned
   around the ideal mirror direction (jittered by an amount proportional to
   `roughness`) instead of one — this directly visualizes the D-term
   "spread of facet normals" idea from microfacet-theory.md, and it's a
   satisfying way to tie the two references together: turn the roughness
   slider and watch the ray fan widen or narrow in lockstep with the
   highlight on the surface.

Keep the ray count modest (dozens, not thousands) — the goal is legibility,
not a real Monte-Carlo estimate. Gate it behind a toggle so it doesn't
clutter the default view, and mention in the UI copy that this is a
*diagram* of the transport model the shader is evaluating analytically, not
a literal simulation running per-pixel.
